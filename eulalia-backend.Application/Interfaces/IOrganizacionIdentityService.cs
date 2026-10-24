using eulalia_backend.Domain.Entities;

namespace eulalia_backend.Application.Interfaces;

public interface IOrganizacionIdentityService
{
    Task<OrganizacionIdentidad> ProvisionAsync(int organizacionId, CancellationToken cancellationToken = default);
    Task<OrganizacionIdentidad> RetryOrganizationCredentialAsync(int organizacionId, CancellationToken cancellationToken = default);
    Task<OrganizacionIdentidad> ConfirmCredentialReceiptAsync(int organizacionId, CancellationToken cancellationToken = default);
    Task<OrganizacionIdentidad?> RefreshCredentialStatusAsync(int organizacionId, CancellationToken cancellationToken = default);
}
