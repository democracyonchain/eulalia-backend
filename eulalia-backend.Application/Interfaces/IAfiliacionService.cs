using System.Threading.Tasks;
using eulalia_backend.Application.DTOs;

namespace eulalia_backend.Application.Interfaces
{
    public interface IAfiliacionService
    {
        Task<IEnumerable<AfiliacionDto>> GetAllAsync();
        Task<AfiliacionDto?> GetByIdAsync(int id);
        Task<AfiliacionDto> CreateAsync(AfiliacionDto dto);
        Task<bool> AnularAsync(int id);
    }
}
