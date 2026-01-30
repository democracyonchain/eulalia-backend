using System.Threading.Tasks;
using eulalia_backend.Application.DTOs;

namespace eulalia_backend.Application.Interfaces
{
    public interface IProvinciaService
    {
        Task<IEnumerable<ProvinciaDto>> GetAllAsync();
        Task<ProvinciaDto?> GetByIdAsync(int id);
        Task<ProvinciaDto> CreateAsync(ProvinciaDto dto);
        Task<bool> UpdateAsync(int id, ProvinciaDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
