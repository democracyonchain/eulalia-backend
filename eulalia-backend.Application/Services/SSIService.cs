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

                var eventType = root.TryGetProperty("eventType", out var et) ? et.GetString() : "";
                var connectionId = root.TryGetProperty("connectionId", out var cid) ? cid.GetString() : "";
                var theirLabel = root.TryGetProperty("theirLabel", out var label) ? label.GetString() : "";

                _logger.LogInformation("Received webhook event: {EventType}, ConnectionId: {ConnectionId}, Label: {Label}", 
                    eventType, connectionId, theirLabel);

                if (string.Equals(eventType, "ConnectionEstablished", StringComparison.OrdinalIgnoreCase))
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
                            
                            var schemaId = "Schema:VoterID:1.0";
                            var offerResult = await _identusClient.CreateCredentialOfferAsync(holderDid, schemaId);
                            
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
                                issuance.HolderDid = holderDid;
                                issuance.Status = SsiIssuanceStatus.CredentialIssued;
                                issuance.UpdatedAt = DateTime.UtcNow;
                                
                                _repository.Update(issuance);
                                await _repository.SaveChangesAsync();

                                _logger.LogWarning("Could not create credential offer for {Cedula}, but connection is established", cedula);
                            }

                            _logger.LogInformation("Connection established and credential issued for {Cedula}", cedula);
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
