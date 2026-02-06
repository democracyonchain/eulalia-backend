using eulalia_backend.Application.DTOs;
using eulalia_backend.Application.Interfaces;
using eulalia_backend.Domain.Entities;

namespace eulalia_backend.Application.Services
{
    public class OrganizacionService : IOrganizacionService
    {
        private readonly IRepository<Organizacion> _repository;

        public OrganizacionService(IRepository<Organizacion> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<OrganizacionDto>> GetAllAsync()
        {
            var data = await _repository.GetAllAsync();
            return data.Select(o => new OrganizacionDto
            {
                OrganizacionId = o.Organizacion_Id,
                Nombre = o.Nombre,
                Tipo = o.Tipo_Organizacion,
                CodigoProvincia = o.Codigo_Provincia,
                CodigoCanton = o.Codigo_Canton,
                CodigoParroquia = o.Codigo_Parroquia,
                ResponsableCedula = o.Responsable_Cedula,
                Estado = o.Estado_Validacion
            });
        }

        public async Task<OrganizacionDto?> GetByIdAsync(int id)
        {
            var o = await _repository.GetByIdAsync(id);
            if (o == null) return null;
            return new OrganizacionDto
            {
                OrganizacionId = o.Organizacion_Id,
                Nombre = o.Nombre,
                Tipo = o.Tipo_Organizacion,
                CodigoProvincia = o.Codigo_Provincia,
                CodigoCanton = o.Codigo_Canton,
                CodigoParroquia = o.Codigo_Parroquia,
                ResponsableCedula = o.Responsable_Cedula,
                Estado = o.Estado_Validacion
            };
        }

        public async Task<OrganizacionDto> CreateAsync(OrganizacionDto dto)
        {
            var entity = new Organizacion
            {
                Nombre = dto.Nombre,
                Tipo_Organizacion = dto.Tipo,
                Codigo_Provincia = dto.CodigoProvincia,
                Codigo_Canton = dto.CodigoCanton,
                Codigo_Parroquia = dto.CodigoParroquia,
                Responsable_Cedula = dto.ResponsableCedula,
                Estado_Validacion = dto.Estado,
                Fecha_Creacion = DateTime.UtcNow
            };
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
            dto.OrganizacionId = entity.Organizacion_Id;
            return dto;
        }

        public async Task<bool> UpdateAsync(int id, OrganizacionDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return false;

            entity.Nombre = dto.Nombre;
            entity.Tipo_Organizacion = dto.Tipo;
            entity.Codigo_Provincia = dto.CodigoProvincia;
            entity.Codigo_Canton = dto.CodigoCanton;
            entity.Codigo_Parroquia = dto.CodigoParroquia;
            entity.Responsable_Cedula = dto.ResponsableCedula;
            entity.Estado_Validacion = dto.Estado;

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
