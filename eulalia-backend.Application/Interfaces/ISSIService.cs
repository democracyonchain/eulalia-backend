using System.Threading.Tasks;
using eulalia_backend.Application.DTOs;

namespace eulalia_backend.Application.Interfaces
{
    public interface ISSIService
    {
        Task<SSIInvitationDto> CreateInvitationAsync(string cedula);
        Task<SSIStatusDto> GetDidStatusAsync(string cedula);
    }
}
