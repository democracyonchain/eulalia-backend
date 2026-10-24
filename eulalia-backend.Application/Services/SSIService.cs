using eulalia_backend.Application.DTOs;
using eulalia_backend.Application.Interfaces;
using eulalia_backend.Domain.Entities;
using eulalia_backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace eulalia_backend.Application.Services
{
    public class SSIService : ISSIService
    {
        private readonly IRepository<SsiIssuance> _repository;
        private readonly IIdentusClient _identusClient;
        private readonly ILogger<SSIService> _logger;

        public SSIService(IRepository<SsiIssuance> repository, IIdentusClient identusClient, ILogger<SSIService> logger)
        {
            _repository = repository;
            _identusClient = identusClient;
            _logger = logger;
        }

        public async Task<SSIInvitationDto> CreateInvitationAsync(string cedula)
        {
            // Idempotency check: look for an active invitation for this cedula
            var existingIssuance = (await _repository.GetAllAsync())
                .FirstOrDefault(s => s.Cedula == cedula && s.Status != SsiIssuanceStatus.Failed);

            if (existingIssuance != null && !string.IsNullOrEmpty(existingIssuance.InvitationUrl))
            {
                _logger.LogInformation("Returning existing invitation for cedula {Cedula}", cedula);
                return new SSIInvitationDto
                {
                    InvitationId = existingIssuance.InvitationId ?? "",
                    InvitationUrl = existingIssuance.InvitationUrl,
                    Status = existingIssuance.Status.ToString(),
                    ExpiresAt = existingIssuance.CreatedAt.AddDays(1) // Assuming 1 day expiry
                };
            }

            _logger.LogInformation("Creating new SSI invitation for cedula {Cedula}", cedula);
            
            try
            {
                var (invitationUrl, invitationId) = await _identusClient.CreateInvitationAsync($"VoterID-{cedula}");

                var issuance = new SsiIssuance
                {
                    Cedula = cedula,
                    InvitationId = invitationId,
                    InvitationUrl = invitationUrl,
                    Status = SsiIssuanceStatus.InvitationGenerated,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _repository.AddAsync(issuance);
                await _repository.SaveChangesAsync();

                return new SSIInvitationDto
                {
                    InvitationId = invitationId,
                    InvitationUrl = invitationUrl,
                    Status = issuance.Status.ToString(),
                    ExpiresAt = issuance.CreatedAt.AddDays(1)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create SSI invitation for {Cedula}", cedula);
                
                var failedIssuance = new SsiIssuance
                {
                    Cedula = cedula,
                    Status = SsiIssuanceStatus.Failed,
                    ErrorMessage = ex.Message,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _repository.AddAsync(failedIssuance);
                await _repository.SaveChangesAsync();
                
                throw;
            }
        }

        public async Task<SSIInvitationDto> RetryInvitationAsync(string cedula)
        {
            var activeIssuances = (await _repository.GetAllAsync())
                .Where(s => s.Cedula == cedula && s.Status != SsiIssuanceStatus.Failed)
                .ToList();

            foreach (var issuance in activeIssuances)
            {
                issuance.Status = SsiIssuanceStatus.Failed;
                issuance.ErrorMessage = "Invitation replaced by an explicit retry request.";
                issuance.UpdatedAt = DateTime.UtcNow;
                _repository.Update(issuance);
            }

            if (activeIssuances.Count > 0)
            {
                await _repository.SaveChangesAsync();
            }

            return await CreateInvitationAsync(cedula);
        }

        public async Task<SSIStatusDto> GetDidStatusAsync(string cedula)
        {
            var issuance = (await _repository.GetAllAsync())
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefault(s => s.Cedula == cedula);

            if (issuance == null)
            {
                return new SSIStatusDto
                {
                    Status = "NotStarted",
                    LastUpdated = DateTime.UtcNow,
                    Details = "No issuance process found for this citizen."
                };
            }

            // If we have a credentialRecordId, we might want to poll Identus for status update
            if (!string.IsNullOrEmpty(issuance.CredentialRecordId) && issuance.Status != SsiIssuanceStatus.CredentialIssued)
            {
                var externalStatus = await _identusClient.GetCredentialRecordStatusAsync(issuance.CredentialRecordId);
                if (!string.IsNullOrEmpty(externalStatus))
                {
                    // Map external status to our enum if needed and update repo
                    _logger.LogInformation("External status for {Cedula}: {Status}", cedula, externalStatus);
                    // issuance.Status = MapStatus(externalStatus);
                    issuance.UpdatedAt = DateTime.UtcNow;
                    _repository.Update(issuance);
                    await _repository.SaveChangesAsync();
                }
            }

            return new SSIStatusDto
            {
                Status = issuance.Status.ToString(),
                LastUpdated = issuance.UpdatedAt,
                Details = issuance.ErrorMessage,
                Error = issuance.Status == SsiIssuanceStatus.Failed ? issuance.ErrorMessage : null
            };
        }

        public async Task HandleWebhookEventAsync(string eventBody)
        {
            try
            {
                var doc = System.Text.Json.JsonDocument.Parse(eventBody);
                var root = doc.RootElement;

                // Cloud Agent events use an envelope; retain the legacy top-level fields for older senders.
                var data = root.TryGetProperty("data", out var eventData) && eventData.ValueKind == System.Text.Json.JsonValueKind.Object
                    ? eventData
                    : root;
                var eventType = root.TryGetProperty("type", out var type)
                    ? type.GetString()
                    : root.TryGetProperty("eventType", out var legacyType) ? legacyType.GetString() : "";
                var connectionId = data.TryGetProperty("connectionId", out var cid)
                    ? cid.GetString()
                    : root.TryGetProperty("connectionId", out var legacyConnectionId) ? legacyConnectionId.GetString() : "";
                var theirLabel = data.TryGetProperty("label", out var label)
                    ? label.GetString()
                    : root.TryGetProperty("theirLabel", out var legacyLabel) ? legacyLabel.GetString() : "";
                var connectionState = data.TryGetProperty("state", out var state) ? state.GetString() : "";

                _logger.LogInformation("Received webhook event: {EventType}, ConnectionId: {ConnectionId}, Label: {Label}, ConnectionState: {ConnectionState}",
                    eventType, connectionId, theirLabel, connectionState);

                var isConnectionEvent = string.Equals(eventType, "ConnectionUpdated", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(eventType, "ConnectionEstablished", StringComparison.OrdinalIgnoreCase);
                var isEstablishedState = string.Equals(connectionState, "ConnectionResponseSent", StringComparison.OrdinalIgnoreCase)
                    // Legacy ConnectionEstablished payloads did not include a separate state field.
                    || (string.IsNullOrEmpty(connectionState) && string.Equals(eventType, "ConnectionEstablished", StringComparison.OrdinalIgnoreCase));

                if (isConnectionEvent && isEstablishedState)
                {
                    var cedula = "";
                    if (!string.IsNullOrEmpty(theirLabel) && theirLabel.StartsWith("VoterID-"))
                    {
                        cedula = theirLabel.Substring(8);
                    }

                    if (!string.IsNullOrEmpty(cedula))
                    {
                        var issuance = (await _repository.GetAllAsync())
                            .FirstOrDefault(s => s.Cedula == cedula && s.Status == SsiIssuanceStatus.InvitationGenerated);

                        if (issuance != null)
                        {
                            var holderDid = $"did:peer:1:{connectionId}";

                            // Claim the invitation before the external call so repeated state events cannot create offers twice.
                            issuance.HolderDid = holderDid;
                            issuance.Status = SsiIssuanceStatus.Requested;
                            issuance.UpdatedAt = DateTime.UtcNow;
                            _repository.Update(issuance);
                            await _repository.SaveChangesAsync();
                            
                            var schemaId = _identusClient.ConfiguredSchemaId;
                            if (string.IsNullOrWhiteSpace(schemaId))
                            {
                                issuance.Status = SsiIssuanceStatus.Failed;
                                issuance.ErrorMessage = "Identus schema configuration is missing; credential offer was not created.";
                                issuance.UpdatedAt = DateTime.UtcNow;
                                _repository.Update(issuance);
                                await _repository.SaveChangesAsync();
                                _logger.LogError("Cannot create credential offer for {Cedula}: Identus schema configuration is missing", cedula);
                                return;
                            }

                             (string RecordId, string OfferUrl)? offerResult;
                             try
                             {
                                 offerResult = await _identusClient.CreateCredentialOfferAsync(
                                     _identusClient.ConfiguredIssuingDid ?? holderDid,
                                     schemaId,
                                     connectionId!,
                                     automaticIssuance: true);
                             }
                             catch (Exception ex)
                             {
                                 issuance.Status = SsiIssuanceStatus.Failed;
                                 issuance.ErrorMessage = ex.Message;
                                 issuance.UpdatedAt = DateTime.UtcNow;

                                 _repository.Update(issuance);
                                 await _repository.SaveChangesAsync();

                                 _logger.LogError(ex, "Could not create credential offer for {Cedula}, issuance marked as failed", cedula);
                                 return;
                             }
                            
                            if (offerResult.HasValue)
                            {
                                issuance.CredentialRecordId = offerResult.Value.RecordId;
                                issuance.HolderDid = holderDid;
                                issuance.Status = SsiIssuanceStatus.CredentialIssued;
                                issuance.UpdatedAt = DateTime.UtcNow;
                                
                                _repository.Update(issuance);
                                await _repository.SaveChangesAsync();

                                _logger.LogInformation("Credential offer created for {Cedula}, RecordId: {RecordId}", 
                                    cedula, offerResult.Value.RecordId);
                            }
                             else
                             {
                                issuance.Status = SsiIssuanceStatus.Failed;
                                issuance.ErrorMessage = "Identus did not create a credential offer.";
                                issuance.UpdatedAt = DateTime.UtcNow;
                                
                                _repository.Update(issuance);
                                await _repository.SaveChangesAsync();

                                _logger.LogError("Could not create credential offer for {Cedula}, issuance marked as failed", cedula);
                            }

                            _logger.LogInformation("Connection established and credential offer processing completed for {Cedula}", cedula);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook event");
                throw;
            }
        }
    }
}
