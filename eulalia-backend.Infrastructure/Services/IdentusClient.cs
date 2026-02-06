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
                // Documentation: https://hyperledger-identus.github.io/docs/
                // Endpoint to create out-of-band invitation
                var requestBody = new
                {
                    label = label
                };

                // Note: The exact path in Identus for OOB invitation is usually /connections/create-invitation
                // or /out-of-band/create-invitation depending on the version and configuration.
                // Based on standard Identus OpenAPI:
                var response = await _httpClient.PostAsJsonAsync("connections/create-invitation", requestBody);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error creating invitation in Identus: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    throw new Exception($"Failed to create invitation in Identus: {response.StatusCode}");
                }

                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                
                // Inspecting the typical response from Identus
                string invitationUrl = result.GetProperty("invitationUrl").GetString() ?? throw new Exception("Invitation URL not found in response");
                string invitationId = result.GetProperty("connectionId").GetString() ?? ""; // connectionId is often used as invitation identifier

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
                // Typical endpoint: /issue-credentials/records/{credentialRecordId}
                var response = await _httpClient.GetAsync($"issue-credentials/records/{credentialRecordId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Could not fetch credential record {Id}: {StatusCode}", credentialRecordId, response.StatusCode);
                    return null;
                }

                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                return result.GetProperty("protocolState").GetString();
                // protocolState could be: RequestReceived, CredentialGenerated, CredentialSent, etc.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while calling Identus GetCredentialRecordStatusAsync");
                return null;
            }
        }
    }
}
