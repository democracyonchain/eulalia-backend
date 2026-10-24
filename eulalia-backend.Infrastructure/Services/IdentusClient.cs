using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using eulalia_backend.Infrastructure.Options;
using eulalia_backend.Application.Interfaces;
using eulalia_backend.Application.DTOs;

namespace eulalia_backend.Infrastructure.Services
{
    public class IdentusClient : IIdentusClient
    {
        private readonly HttpClient _httpClient;
        private readonly IdentusOptions _options;
        private readonly ILogger<IdentusClient> _logger;
        public bool OrganizationCredentialConfigurationAvailable =>
            !string.IsNullOrWhiteSpace(_options.OrganizationSchemaId) &&
            !string.IsNullOrWhiteSpace(_options.OrganizationIssuingDid);
        public bool OrganizationGlobalHolderConnectionAvailable =>
            !string.IsNullOrWhiteSpace(_options.OrganizationHolderConnectionId);
        public string? ConfiguredSchemaId => _options.OrganizationSchemaId;
        public string? ConfiguredIssuingDid => _options.OrganizationIssuingDid;

        public IdentusClient(HttpClient httpClient, IOptions<IdentusOptions> options, ILogger<IdentusClient> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
            
            _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
            _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
            
            if (!string.IsNullOrEmpty(_options.ApiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("apikey", _options.ApiKey);
            }
        }

        public async Task<(string InvitationUrl, string InvitationId)> CreateInvitationAsync(string label)
        {
            try
            {
                var requestBody = new
                {
                    label = label
                };

                // Cloud Agent 1.39 endpoint for creating OOB connection invitations.
                var response = await _httpClient.PostAsJsonAsync("connections", requestBody);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error creating invitation in Identus: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    throw new Exception($"Failed to create invitation in Identus: {response.StatusCode}");
                }

                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                
                var invitationId = result.TryGetProperty("connectionId", out var connectionId)
                    ? connectionId.GetString() ?? ""
                    : "";

                string? invitationUrl = null;
                if (result.TryGetProperty("invitationUrl", out var topLevelInvitationUrl))
                {
                    invitationUrl = topLevelInvitationUrl.GetString();
                }
                else if (result.TryGetProperty("invitation", out var invitation) &&
                         invitation.TryGetProperty("invitationUrl", out var nestedInvitationUrl))
                {
                    invitationUrl = nestedInvitationUrl.GetString();
                }

                if (string.IsNullOrWhiteSpace(invitationUrl))
                    throw new Exception("Invitation URL not found in Identus response.");

                if (string.IsNullOrWhiteSpace(invitationId))
                    throw new Exception("Identus no devolvió una referencia de conexión.");

                return (invitationUrl, invitationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while calling Identus CreateInvitationAsync");
                throw;
            }
        }

        public async Task<string?> GetCredentialRecordStatusAsync(string credentialRecordId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"issue-credentials/records/{credentialRecordId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Could not fetch credential record {Id}: {StatusCode}", credentialRecordId, response.StatusCode);
                    return null;
                }

                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                return result.GetProperty("protocolState").GetString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while calling Identus GetCredentialRecordStatusAsync");
                return null;
            }
        }

        public async Task<(string RecordId, string OfferUrl)?> CreateCredentialOfferAsync(string holderDid, string schemaId)
            => await CreateCredentialOfferInternalAsync(holderDid, schemaId, holderDid, automaticIssuance: false);

        public async Task<(string RecordId, string OfferUrl)?> CreateCredentialOfferAsync(
            string holderDid,
            string schemaId,
            string holderConnectionId)
            => await CreateCredentialOfferInternalAsync(holderConnectionId, schemaId, holderDid, automaticIssuance: false);

        public async Task<(string RecordId, string OfferUrl)?> CreateCredentialOfferAsync(
            string holderDid,
            string schemaId,
            string holderConnectionId,
            bool automaticIssuance)
            => await CreateCredentialOfferInternalAsync(holderConnectionId, schemaId, holderDid, automaticIssuance);

        private async Task<(string RecordId, string OfferUrl)?> CreateCredentialOfferInternalAsync(
            string connectionId,
            string schemaId,
            string organizationDid,
            bool automaticIssuance)
        {
            try
            {
                var requestBody = new
                {
                    connectionId,
                    claims = new { organizationDid },
                    schemaId = schemaId,
                    issuingDID = _options.OrganizationIssuingDid,
                    automaticIssuance
                };

                var response = await _httpClient.PostAsJsonAsync("issue-credentials/credential-offers", requestBody);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var safeErrorContent = SanitizeErrorBody(errorContent);
                    _logger.LogError("Error creating credential offer: {StatusCode} - {Error}", response.StatusCode, safeErrorContent);
                    throw new HttpRequestException(
                        $"Identus credential offer failed with HTTP {(int)response.StatusCode}: {safeErrorContent}");
                }

                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                
                var recordId = FindString(result, "recordId", "id") ?? "";
                var offerUrl = FindString(result, "offerUrl", "invitationUrl") ?? "";

                return (recordId, offerUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while calling Identus CreateCredentialOfferAsync");
                throw;
            }
        }

        public async Task<string?> IssueCredentialAsync(string recordId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"issue-credentials/records/{recordId}/issue-credential", null);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error issuing credential: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    return null;
                }

                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                return result.TryGetProperty("protocolState", out var state) ? state.GetString() : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while calling Identus IssueCredentialAsync");
                return null;
            }
        }

        public async Task<IdentusConnectionStatus?> GetConnectionStatusAsync(
            string connectionId,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync(
                $"connections/{Uri.EscapeDataString(connectionId)}",
                cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Identus returned {(int)response.StatusCode} while reading a connection.");

            using var document = ParseJson(body, "connection status");
            var root = document.RootElement;
            return new IdentusConnectionStatus
            {
                ConnectionId = FindString(root, "connectionId") ?? connectionId,
                MyDid = FindString(root, "myDid"),
                TheirDid = FindString(root, "theirDid"),
                State = FindString(root, "state", "protocolState")
            };
        }

        public Task<(string RecordId, string OfferUrl)?> CreateOrganizationCredentialOfferAsync(string holderDid, string? holderConnectionId = null, CancellationToken cancellationToken = default)
        {
            if (!OrganizationCredentialConfigurationAvailable)
                return Task.FromResult<(string RecordId, string OfferUrl)?>(null);
            var connectionId = string.IsNullOrWhiteSpace(holderConnectionId)
                ? _options.OrganizationHolderConnectionId
                : holderConnectionId;
            if (string.IsNullOrWhiteSpace(connectionId))
                return Task.FromResult<(string RecordId, string OfferUrl)?>(null);
            return CreateCredentialOfferInternalAsync(connectionId, _options.OrganizationSchemaId!, holderDid, automaticIssuance: false);
        }

        public async Task<IdentusDidResult> CreateManagedDidAsync(CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("did-registrar/dids", new
            {
                documentTemplate = new
                {
                    publicKeys = new[]
                    {
                        new { id = "auth-1", purpose = "authentication" },
                        new { id = "issue-1", purpose = "assertionMethod" }
                    },
                    services = Array.Empty<object>()
                }
            }, cancellationToken);

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            EnsureSuccess(response, body, "creating managed DID");
            using var document = ParseJson(body, "managed DID");
            var longFormDid = FindString(document.RootElement, "longFormDid");
            if (string.IsNullOrWhiteSpace(longFormDid))
                throw new InvalidOperationException("Identus DID response did not contain longFormDid.");

            return new IdentusDidResult(longFormDid);
        }

        public async Task<IdentusPublicationResult> RequestDidPublicationAsync(string longFormDid, CancellationToken cancellationToken = default)
        {
            var path = $"did-registrar/dids/{Uri.EscapeDataString(longFormDid)}/publications";
            var response = await _httpClient.PostAsync(path, null, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            EnsureSuccess(response, body, "requesting DID publication");
            using var document = ParseJson(body, "DID publication");
            var operationId = FindString(document.RootElement, "operationId", "publicationId", "operationRef", "id");
            if (string.IsNullOrWhiteSpace(operationId))
                throw new InvalidOperationException("Identus publication response did not contain an operation reference.");

            var publishedDid = FindString(document.RootElement, "publishedPrismDID", "publishedDid", "shortFormDid");
            return new IdentusPublicationResult(operationId, publishedDid);
        }

        private void EnsureSuccess(HttpResponseMessage response, string body, string operation)
        {
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Identus error while {Operation}: {StatusCode} - {Body}", operation, response.StatusCode, body);
                throw new HttpRequestException($"Identus returned {(int)response.StatusCode} while {operation}.");
            }
        }

        private static JsonDocument ParseJson(string body, string operation)
        {
            try
            {
                return JsonDocument.Parse(body);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Identus returned malformed JSON while {operation}.", ex);
            }
        }

        private static string SanitizeErrorBody(string body)
        {
            const int maxResponseLength = 1000;
            var sanitized = Regex.Replace(
                body,
                "(?i)(\\\"(?:api[-_ ]?key|authorization|access[-_ ]?token|refresh[-_ ]?token|token|secret|password)\\\"\\s*:\\s*\\\")[^\\\"]*(\\\")",
                "$1[REDACTED]$2");
            sanitized = Regex.Replace(
                sanitized,
                "(?i)((?:api[-_ ]?key|authorization|access[-_ ]?token|refresh[-_ ]?token|token|secret|password)\\s*[:=]\\s*)[^,;\\s]+",
                "$1[REDACTED]");

            return sanitized.Length <= maxResponseLength
                ? sanitized
                : sanitized[..maxResponseLength] + "...";
        }

        private static string? FindString(JsonElement element, params string[] names)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var name in names)
                {
                    if (element.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String)
                        return property.GetString();
                }

                foreach (var property in element.EnumerateObject())
                {
                    var value = FindString(property.Value, names);
                    if (!string.IsNullOrWhiteSpace(value)) return value;
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var child in element.EnumerateArray())
                {
                    var value = FindString(child, names);
                    if (!string.IsNullOrWhiteSpace(value)) return value;
                }
            }

            return null;
        }
    }
}
