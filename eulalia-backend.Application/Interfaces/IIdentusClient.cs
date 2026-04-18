using System.Threading.Tasks;

namespace eulalia_backend.Application.Interfaces
{
    public interface IIdentusClient
    {
        Task<(string InvitationUrl, string InvitationId)> CreateInvitationAsync(string label);
        Task<string?> GetCredentialRecordStatusAsync(string credentialRecordId);
        
        Task<(string RecordId, string OfferUrl)?> CreateCredentialOfferAsync(string holderDid, string schemaId);
        Task<string?> IssueCredentialAsync(string recordId);
    }
}