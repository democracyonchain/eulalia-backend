using eulalia_backend.Application.DTOs;
using eulalia_backend.Application.Interfaces;
using eulalia_backend.Domain.Entities;

namespace eulalia_backend.Application.Services
{
    public class ProvinciaService : IProvinciaService
    {
        private readonly IRepository<Provincia> _repository;

        public ProvinciaService(IRepository<Provincia> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ProvinciaDto>> GetAllAsync()
        {
            var data = await _repository.GetAllAsync();
            return data.Select(p => new ProvinciaDto
            {
                Codigo = p.Codigo_Provincia,
                Nombre = p.Nombre
            });
        }

        public async Task<ProvinciaDto?> GetByIdAsync(int id)
        {
            var p = await _repository.GetByIdAsync(id);
            if (p == null) return null;
            return new ProvinciaDto
            {
                Codigo = p.Codigo_Provincia,
                Nombre = p.Nombre
            };
        }

        public async Task<ProvinciaDto> CreateAsync(ProvinciaDto dto)
        {
            var entity = new Provincia
            {
                Codigo_Provincia = dto.Codigo,
                Nombre = dto.Nombre
            };
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
            return dto;
        }

        public async Task<bool> UpdateAsync(int id, ProvinciaDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return false;

            entity.Nombre = dto.Nombre;

            _repository.Update(entity);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return false;

            _repository.Remove(entity);
            await _repository.SaveChangesAsync();
            return true;
        }
    }
}
