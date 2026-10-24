using eulalia_backend.Application.DTOs;

namespace eulalia_backend.Application.Interfaces;

public interface IIdentusClient
{
    // Shared schema registry identifier used by citizen and organization issuance.
    string? ConfiguredSchemaId { get; }
    string? ConfiguredIssuingDid { get; }
    Task<(string InvitationUrl, string InvitationId)> CreateInvitationAsync(string label);
    Task<IdentusConnectionStatus?> GetConnectionStatusAsync(string connectionId, CancellationToken cancellationToken = default);
    Task<string?> GetCredentialRecordStatusAsync(string credentialRecordId);
    Task<(string RecordId, string OfferUrl)?> CreateCredentialOfferAsync(string holderDid, string schemaId);
    Task<(string RecordId, string OfferUrl)?> CreateCredentialOfferAsync(string holderDid, string schemaId, string holderConnectionId);
    Task<(string RecordId, string OfferUrl)?> CreateCredentialOfferAsync(string holderDid, string schemaId, string holderConnectionId, bool automaticIssuance);
    Task<string?> IssueCredentialAsync(string recordId);
    Task<IdentusDidResult> CreateManagedDidAsync(CancellationToken cancellationToken = default);
    Task<IdentusPublicationResult> RequestDidPublicationAsync(string longFormDid, CancellationToken cancellationToken = default);
    bool OrganizationCredentialConfigurationAvailable { get; }
    bool OrganizationGlobalHolderConnectionAvailable { get; }
    Task<(string RecordId, string OfferUrl)?> CreateOrganizationCredentialOfferAsync(string holderDid, string? holderConnectionId = null, CancellationToken cancellationToken = default);
}
