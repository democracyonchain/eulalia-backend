using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using eulalia_backend.Infrastructure.Options;
using eulalia_backend.Application.Interfaces;

namespace eulalia_backend.Infrastructure.Services
{
    public class IdentusClient : IIdentusClient
    {
        private readonly HttpClient _httpClient;
        private readonly IdentusOptions _options;
        private readonly ILogger<IdentusClient> _logger;

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
        {
            try
            {
                var requestBody = new
                {
                    holderId = holderDid,
                    claims = new { },
                    schemaId = schemaId,
                    credentialDefinitionId = schemaId
                };

                var response = await _httpClient.PostAsJsonAsync("issue-credentials/credential-offers", requestBody);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error creating credential offer: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    return null;
                }

                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                
                var recordId = result.TryGetProperty("recordId", out var rid) ? rid.GetString() ?? "" : "";
                var offerUrl = result.TryGetProperty("offerUrl", out var ourl) ? ourl.GetString() ?? "" : "";

                return (recordId, offerUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while calling Identus CreateCredentialOfferAsync");
                return null;
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
    }
}
