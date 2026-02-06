using System.Threading.Tasks;
using eulalia_backend.Application.DTOs;

namespace eulalia_backend.Application.Interfaces
{
    public interface ICiudadanoService
    {
        Task<IEnumerable<CiudadanoDto>> GetAllAsync();
        Task<CiudadanoDto?> GetByCedulaAsync(string cedula);
        Task<CiudadanoDto> CreateAsync(CiudadanoDto dto);
    }
}
