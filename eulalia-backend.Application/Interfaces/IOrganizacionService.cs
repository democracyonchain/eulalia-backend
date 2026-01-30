using System.Threading.Tasks;
using eulalia_backend.Application.DTOs;

namespace eulalia_backend.Application.Interfaces
{
    public interface IOrganizacionService
    {
        Task<IEnumerable<OrganizacionDto>> GetAllAsync();
        Task<OrganizacionDto?> GetByIdAsync(int id);
        Task<OrganizacionDto> CreateAsync(OrganizacionDto dto);
        Task<bool> UpdateAsync(int id, OrganizacionDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
