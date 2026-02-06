using eulalia_backend.Application.DTOs;
using eulalia_backend.Application.Interfaces;
using eulalia_backend.Domain.Entities;

namespace eulalia_backend.Application.Services
{
    public class AfiliacionService : IAfiliacionService
    {
        private readonly IRepository<Afiliacion> _repository;

        public AfiliacionService(IRepository<Afiliacion> repository)
        {
            _repository = repository;
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
                    Estado = a.Estado
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
                Estado = a.Estado
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
            
            dto.AfiliacionId = afiliacion.Afiliacion_Id;
            return dto;
        }

        public async Task<bool> AnularAsync(int id)
        {
            var a = await _repository.GetByIdAsync(id);
            if (a == null) return false;

            a.Estado = "Anulado";
            _repository.Update(a);
            await _repository.SaveChangesAsync();
            return true;
        }
    }
}
