using eulalia_backend.Application.DTOs;
using eulalia_backend.Application.Interfaces;
using eulalia_backend.Domain.Entities;

namespace eulalia_backend.Application.Services
{
    public class SSIService : ISSIService
    {
        private readonly IRepository<SSI> _repository;

        public SSIService(IRepository<SSI> repository)
        {
            _repository = repository;
        }

        public async Task<SSIInvitationDto> CreateInvitationAsync(string cedula)
        {
            // FAKE IMPLEMENTATION FOR PHASE 1.5 - MOBILE READY
            // In Phase 2, this will call the Identus Cloud Agent
            var invitationId = Guid.NewGuid().ToString();
            
            return await Task.FromResult(new SSIInvitationDto
            {
                InvitationId = invitationId,
                InvitationUrl = $"https://eulalia.vote/invite?id={invitationId}&cedula={cedula}",
                QrCodeBase64 = "BASE64_SIMULATED_QR_CODE"
            });
        }

        public async Task<string> GetDidStatusAsync(string cedula)
        {
            var ssi = (await _repository.GetAllAsync())
                .FirstOrDefault(s => s.Cedula == cedula);
                
            return ssi?.Estado ?? "no_solicitado";
        }
    }
}
