using eulalia_backend.Application.Interfaces;
using eulalia_backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace eulalia_backend.Application.Services;

public class OrganizacionIdentityService : IOrganizacionIdentityService
{
    private static readonly TimeSpan CredentialStatusTimeout = TimeSpan.FromSeconds(3);
    private readonly IRepository<Organizacion> _organizaciones;
    private readonly IRepository<OrganizacionIdentidad> _identidades;
    private readonly IIdentusClient _identus;
    private readonly ILogger<OrganizacionIdentityService> _logger;

    public OrganizacionIdentityService(
        IRepository<Organizacion> organizaciones,
        IRepository<OrganizacionIdentidad> identidades,
        IIdentusClient identus,
        ILogger<OrganizacionIdentityService> logger)
    {
        _organizaciones = organizaciones;
        _identidades = identidades;
        _identus = identus;
        _logger = logger;
    }

    public async Task<OrganizacionIdentidad> ProvisionAsync(int organizacionId, CancellationToken cancellationToken = default)
    {
        var organizacion = await _organizaciones.GetByIdAsync(organizacionId)
            ?? throw new InvalidOperationException($"Organization {organizacionId} was not found.");
        var existing = (await _identidades.FindAsync(i => i.OrganizacionId == organizacionId)).SingleOrDefault();

        if (existing == null)
        {
            existing = new OrganizacionIdentidad
            {
                OrganizacionId = organizacionId,
                Estado = "pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _identidades.AddAsync(existing);
            await _identidades.SaveChangesAsync();
        }

        if (existing.Estado == "completed" ||
            (existing.Estado == "wallet_not_connected" && string.IsNullOrWhiteSpace(existing.ClientDid)) ||
            (existing.Estado == "credential_pending" && !string.IsNullOrWhiteSpace(existing.CredentialRecordId)))
            return existing;

        try
        {
            if (string.IsNullOrWhiteSpace(existing.ClientDid))
            {
                existing.Estado = "wallet_not_connected";
                existing.CredentialStatus = "credential_pending";
                existing.ErrorMessage = "La wallet de la organización no está conectada. Asocie un DID público desde la wallet antes de pre-aprobar.";
                await SaveAsync(existing);
                return existing;
            }

            if (!_identus.OrganizationCredentialConfigurationAvailable ||
                (string.IsNullOrWhiteSpace(existing.HolderConnectionId) && !_identus.OrganizationGlobalHolderConnectionAvailable))
            {
                existing.Estado = "credential_pending";
                existing.CredentialStatus = "credential_pending";
                existing.ErrorMessage = "Identus requiere OrganizationSchemaId, OrganizationHolderConnectionId y OrganizationIssuingDid. La wallet controla el DID; no se creó una identidad administrada alternativa.";
                await SaveAsync(existing);
                return existing;
            }

            if (!string.IsNullOrWhiteSpace(existing.HolderConnectionId) &&
                !await IsEstablishedForWalletAsync(existing.HolderConnectionId, cancellationToken))
            {
                existing.Estado = "credential_pending";
                existing.CredentialStatus = "credential_pending";
                existing.ErrorMessage = "La conexión holder guardada no está establecida en Identus; se requiere recuperar la conexión antes de emitir.";
                await SaveAsync(existing);
                return existing;
            }

            var offer = await _identus.CreateOrganizationCredentialOfferAsync(existing.ClientDid, existing.HolderConnectionId, cancellationToken);
            if (offer is null || string.IsNullOrWhiteSpace(offer.Value.RecordId))
                throw new InvalidOperationException("Identus no devolvió un registro de oferta de credencial.");

            existing.CredentialRecordId = offer.Value.RecordId;
            existing.CredentialStatus = "offer_pending_holder_acceptance";
            existing.Estado = "credential_pending";
            existing.ErrorMessage = "Oferta de credencial creada en Identus; falta la aceptación del holder. No se marca como emitida hasta confirmar ese estado.";
            await SaveAsync(existing);
            return existing;
        }
        catch (Exception ex)
        {
            existing.Estado = "failed";
            existing.ErrorMessage = SanitizeError(ex.Message);
            await SaveAsync(existing);
            _logger.LogError(ex, "Organization identity provisioning failed for {OrganizationId}", organizacionId);
            throw;
        }
    }

    public async Task<OrganizacionIdentidad> RetryOrganizationCredentialAsync(
        int organizacionId,
        CancellationToken cancellationToken = default)
    {
        _ = await _organizaciones.GetByIdAsync(organizacionId)
            ?? throw new InvalidOperationException($"Organization {organizacionId} was not found.");
        var identity = (await _identidades.FindAsync(i => i.OrganizacionId == organizacionId)).SingleOrDefault()
            ?? throw new InvalidOperationException("La identidad de la organización todavía no está configurada.");

        if (identity.Estado == "completed")
            throw new InvalidOperationException("La credencial de la organización ya fue completada.");
        if (identity.Estado != "credential_pending")
            throw new InvalidOperationException("La identidad de la organización no está en un estado pendiente elegible para retry.");
        if (identity.CredentialStatus is "accepted" or "issued" or "completed")
            throw new InvalidOperationException("La credencial de la organización ya fue aceptada.");
        if (string.IsNullOrWhiteSpace(identity.ClientDid))
            throw new InvalidOperationException("La organización no tiene un DID de wallet asociado.");
        if (!_identus.OrganizationCredentialConfigurationAvailable ||
            (string.IsNullOrWhiteSpace(identity.HolderConnectionId) && !_identus.OrganizationGlobalHolderConnectionAvailable))
            throw new InvalidOperationException("La configuración de credencial o la conexión holder no está disponible.");
        if (identity.CredentialStatus == "offer_pending_holder_acceptance")
        {
            if (string.IsNullOrWhiteSpace(identity.HolderConnectionId) ||
                !await IsEstablishedForWalletAsync(identity.HolderConnectionId, cancellationToken))
                throw new InvalidOperationException("La conexión holder debe recuperarse y validarse antes de crear una nueva oferta.");
        }

        if (!string.IsNullOrWhiteSpace(identity.HolderConnectionId) &&
            !await IsEstablishedForWalletAsync(identity.HolderConnectionId, cancellationToken))
            throw new InvalidOperationException("La conexión holder guardada no está establecida en Identus; recuperá la conexión antes de reofertar.");

        var offer = await _identus.CreateOrganizationCredentialOfferAsync(
            identity.ClientDid,
            identity.HolderConnectionId,
            cancellationToken);
        if (offer is null || string.IsNullOrWhiteSpace(offer.Value.RecordId))
            throw new InvalidOperationException("Identus no devolvió un nuevo registro de oferta de credencial.");

        identity.CredentialRecordId = offer.Value.RecordId;
        identity.CredentialStatus = "offer_pending_holder_acceptance";
        identity.Estado = "credential_pending";
        identity.ErrorMessage = "Se creó una nueva oferta de credencial; aceptala desde la wallet de la organización.";
        await SaveAsync(identity);
        return identity;
    }

    public async Task<OrganizacionIdentidad?> RefreshCredentialStatusAsync(
        int organizacionId,
        CancellationToken cancellationToken = default)
    {
        var identity = (await _identidades.FindAsync(i => i.OrganizacionId == organizacionId)).SingleOrDefault();
        if (identity == null || string.IsNullOrWhiteSpace(identity.CredentialRecordId) || identity.Estado == "completed")
            return identity;

        try
        {
            var statusTask = _identus.GetCredentialRecordStatusAsync(identity.CredentialRecordId);
            var completedTask = await Task.WhenAny(
                statusTask,
                Task.Delay(CredentialStatusTimeout, cancellationToken));

            if (completedTask != statusTask)
            {
                _ = statusTask.ContinueWith(
                    task => _ = task.Exception,
                    CancellationToken.None,
                    TaskContinuationOptions.OnlyOnFaulted,
                    TaskScheduler.Default);

                if (!cancellationToken.IsCancellationRequested)
                {
                    identity.ErrorMessage = "No se pudo verificar temporalmente el estado de la credencial en Identus.";
                    await SaveAsync(identity);
                }

                return identity;
            }

            var protocolState = await statusTask;
            if (string.IsNullOrWhiteSpace(protocolState))
            {
                identity.ErrorMessage = "Identus no devolvió el estado de la credencial; se conserva el estado actual.";
                await SaveAsync(identity);
                return identity;
            }

            if (protocolState.Trim().ToLowerInvariant() is "requestreceived" or "request_received" or "request-received")
            {
                var issued = await _identus.IssueCredentialAsync(identity.CredentialRecordId);
                if (!string.IsNullOrWhiteSpace(issued))
                {
                    identity.CredentialStatus = "credential_issued_pending_holder_receipt";
                    identity.ErrorMessage = "La credencial fue emitida por Identus; esperando confirmación de recepción de la wallet.";
                    await SaveAsync(identity);
                }

                return identity;
            }

            if (!IsCredentialAccepted(protocolState))
                return identity;

            identity.CredentialStatus = "issued";
            identity.Estado = "completed";
            identity.ErrorMessage = "Credencial emitida y aceptada por la wallet de la organización.";
            await SaveAsync(identity);
            return identity;
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            identity.ErrorMessage = "No se pudo verificar temporalmente el estado de la credencial en Identus.";
            await SaveAsync(identity);
            _logger.LogWarning(ex, "Could not refresh organization credential status for {OrganizationId}", organizacionId);
            return identity;
        }
    }

    public async Task<OrganizacionIdentidad> ConfirmCredentialReceiptAsync(
        int organizacionId,
        CancellationToken cancellationToken = default)
    {
        var identity = (await _identidades.FindAsync(i => i.OrganizacionId == organizacionId)).SingleOrDefault()
            ?? throw new InvalidOperationException("La identidad de la organización no existe.");

        if (string.IsNullOrWhiteSpace(identity.CredentialRecordId))
            throw new InvalidOperationException("La organización todavía no tiene una credencial emitida.");

        if (identity.CredentialStatus is not ("credential_issued_pending_holder_receipt" or "issued" or "completed"))
            throw new InvalidOperationException("La credencial todavía no está pendiente de confirmación de recepción.");

        identity.CredentialStatus = "issued";
        identity.Estado = "completed";
        identity.ErrorMessage = "Credencial emitida y recibida por la wallet de la organización.";
        await SaveAsync(identity);
        return identity;
    }

    private static bool IsCredentialAccepted(string protocolState) =>
        protocolState.Trim().ToLowerInvariant() is
            "accepted" or
            "credential-issued" or
            "credential_received" or
            "credential-received" or
            "issued" or
            "done";

    private async Task<bool> IsEstablishedForWalletAsync(
        string connectionId,
        CancellationToken cancellationToken)
    {
        var connection = await _identus.GetConnectionStatusAsync(connectionId, cancellationToken);
        return connection is not null &&
            (string.Equals(connection.State, "ConnectionResponseSent", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(connection.State, "ConnectionResponseReceived", StringComparison.OrdinalIgnoreCase));
    }

    private static string SanitizeError(string message)
    {
        var cleaned = new string(message.Where(c => !char.IsControl(c) || c is '\n' or '\r').ToArray());
        return cleaned[..Math.Min(cleaned.Length, 1000)];
    }

    private async Task SaveAsync(OrganizacionIdentidad identity)
    {
        identity.UpdatedAt = DateTime.UtcNow;
        await _identidades.SaveChangesAsync();
    }
}
