using eulalia_backend.Application.DTOs;
using eulalia_backend.Application.Interfaces;
using eulalia_backend.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace eulalia_backend.Application.Services
{
    public class AfiliacionService : IAfiliacionService
    {
        private readonly IRepository<Afiliacion> _repository;
        private readonly ICardanoClient _cardano;

        public AfiliacionService(IRepository<Afiliacion> repository, ICardanoClient? cardano = null)
        {
            _repository = repository;
            _cardano = cardano ?? new DisabledCardanoClient();
        }

        public async Task<IEnumerable<AfiliacionDto>> GetAllAsync()
        {
            var data = await _repository.GetAllAsync();
            return data
                .Select(a => new AfiliacionDto
                {
                    AfiliacionId = a.Afiliacion_Id,
                    Cedula = a.Cedula,
                    OrganizacionId = a.OrganizacionId,
                    FechaAfiliacion = a.FechaAfiliacion,
                    Estado = a.Estado,
                    CardanoEventId = a.Estado == "Anulado" ? a.CardanoCancellationEventId : a.CardanoAffiliationEventId,
                    CardanoEventType = a.Estado == "Anulado" ? "cancellation" : "affiliation",
                    CardanoStatus = a.Estado == "Anulado" ? a.CardanoCancellationStatus : a.CardanoAffiliationStatus,
                    CardanoTxHash = a.Estado == "Anulado" ? a.CardanoCancellationTxHash : a.CardanoAffiliationTxHash
                });
        }

        public async Task<AfiliacionDto?> GetByIdAsync(int id)
        {
            var a = await _repository.GetByIdAsync(id);
            if (a == null) return null;

            return new AfiliacionDto
            {
                AfiliacionId = a.Afiliacion_Id,
                Cedula = a.Cedula,
                OrganizacionId = a.OrganizacionId,
                FechaAfiliacion = a.FechaAfiliacion,
                Estado = a.Estado,
                CardanoEventId = a.Estado == "Anulado" ? a.CardanoCancellationEventId : a.CardanoAffiliationEventId,
                CardanoEventType = a.Estado == "Anulado" ? "cancellation" : "affiliation",
                CardanoStatus = a.Estado == "Anulado" ? a.CardanoCancellationStatus : a.CardanoAffiliationStatus,
                CardanoTxHash = a.Estado == "Anulado" ? a.CardanoCancellationTxHash : a.CardanoAffiliationTxHash
            };
        }

        public async Task<AfiliacionDto> CreateAsync(AfiliacionDto dto)
        {
            var afiliacion = new Afiliacion
            {
                Cedula = dto.Cedula,
                OrganizacionId = dto.OrganizacionId,
                FechaAfiliacion = dto.FechaAfiliacion.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(dto.FechaAfiliacion, DateTimeKind.Utc) 
                    : dto.FechaAfiliacion,
                Estado = "activa"
            };

            await _repository.AddAsync(afiliacion);
            await _repository.SaveChangesAsync();

            var recordHash = CreateRecordHash("affiliation", afiliacion.Afiliacion_Id, afiliacion.OrganizacionId);
            afiliacion.CardanoAffiliationEventId = $"affiliation-{afiliacion.Afiliacion_Id}";
            afiliacion.CardanoAffiliationRecordHash = recordHash;
            afiliacion.CardanoAffiliationStatus = "pending";
            ApplyCardanoResult(
                afiliacion,
                await SubmitSafelyAsync(() => _cardano.SubmitAffiliationAsync(
                    afiliacion.Afiliacion_Id, afiliacion.OrganizacionId, recordHash)),
                false);
            _repository.Update(afiliacion);
            await _repository.SaveChangesAsync();

            return ToDto(afiliacion);
        }

        public async Task<bool> AnularAsync(int id)
        {
            var a = await _repository.GetByIdAsync(id);
            if (a == null) return false;

            if (a.FechaAfiliacion.Kind == DateTimeKind.Unspecified)
            {
                a.FechaAfiliacion = DateTime.SpecifyKind(a.FechaAfiliacion, DateTimeKind.Utc);
            }

            a.Estado = "Anulado";
            var recordHash = CreateRecordHash("cancellation", a.Afiliacion_Id, a.OrganizacionId);
            a.CardanoCancellationEventId = $"cancellation-{a.Afiliacion_Id}";
            a.CardanoCancellationRecordHash = recordHash;
            a.CardanoCancellationStatus = "pending";
            _repository.Update(a);
            await _repository.SaveChangesAsync();

            ApplyCardanoResult(
                a,
                await SubmitSafelyAsync(() => _cardano.SubmitCancellationAsync(
                    a.Afiliacion_Id, a.OrganizacionId, recordHash)),
                true);
            _repository.Update(a);
            await _repository.SaveChangesAsync();
            return true;
        }

        private async Task<CardanoSubmissionResult> SubmitSafelyAsync(Func<Task<CardanoSubmissionResult>> submit)
        {
            try
            {
                return await submit();
            }
            catch
            {
                return new CardanoSubmissionResult(string.Empty, "failed", ErrorCode: "CARDANO_CLIENT_FAILURE");
            }
        }

        private static void ApplyCardanoResult(Afiliacion affiliation, CardanoSubmissionResult result, bool cancellation)
        {
            if (cancellation)
            {
                affiliation.CardanoCancellationEventId = string.IsNullOrWhiteSpace(result.EventId)
                    ? affiliation.CardanoCancellationEventId : result.EventId;
                affiliation.CardanoCancellationStatus = result.Status;
                affiliation.CardanoCancellationTxHash = result.TxHash;
            }
            else
            {
                affiliation.CardanoAffiliationEventId = string.IsNullOrWhiteSpace(result.EventId)
                    ? affiliation.CardanoAffiliationEventId : result.EventId;
                affiliation.CardanoAffiliationStatus = result.Status;
                affiliation.CardanoAffiliationTxHash = result.TxHash;
            }
        }

        private static string CreateRecordHash(string eventType, int affiliationId, int organizationId)
        {
            var canonical = $"eulalia|{eventType}|{affiliationId}|{organizationId}";
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
        }

        private static AfiliacionDto ToDto(Afiliacion a) => new()
        {
            AfiliacionId = a.Afiliacion_Id,
            Cedula = a.Cedula,
            OrganizacionId = a.OrganizacionId,
            FechaAfiliacion = a.FechaAfiliacion,
            Estado = a.Estado,
            CardanoEventId = a.Estado == "Anulado" ? a.CardanoCancellationEventId : a.CardanoAffiliationEventId,
            CardanoEventType = a.Estado == "Anulado" ? "cancellation" : "affiliation",
            CardanoStatus = a.Estado == "Anulado" ? a.CardanoCancellationStatus : a.CardanoAffiliationStatus,
            CardanoTxHash = a.Estado == "Anulado" ? a.CardanoCancellationTxHash : a.CardanoAffiliationTxHash
        };

        private sealed class DisabledCardanoClient : ICardanoClient
        {
            public Task<CardanoSubmissionResult> SubmitAffiliationAsync(int id, int organizationId, string hash, CancellationToken cancellationToken = default) =>
                Task.FromResult(new CardanoSubmissionResult($"affiliation-{id}", "pending", ErrorCode: "CARDANO_NOT_CONFIGURED"));

            public Task<CardanoSubmissionResult> SubmitCancellationAsync(int id, int organizationId, string hash, CancellationToken cancellationToken = default) =>
                Task.FromResult(new CardanoSubmissionResult($"cancellation-{id}", "pending", ErrorCode: "CARDANO_NOT_CONFIGURED"));
        }
    }
}
