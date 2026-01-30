using eulalia_backend.Application.DTOs;
using eulalia_backend.Application.Interfaces;
using eulalia_backend.Domain.Entities;

namespace eulalia_backend.Application.Services
{
    public class CiudadanoService : ICiudadanoService
    {
        private readonly IRepository<Ciudadano> _repository;

        public CiudadanoService(IRepository<Ciudadano> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<CiudadanoDto>> GetAllAsync()
        {
            var ciudadanos = await _repository.GetAllAsync();
            return ciudadanos
                .Select(c => new CiudadanoDto
                {
                    Cedula = c.Cedula,
                    Nombres = c.Nombre,
                    Apellidos = c.Apellido,
                    Telefono = c.Telefono,
                    FechaNacimiento = c.Fecha_Nacimiento ?? DateTime.MinValue
                });
        }

        public async Task<CiudadanoDto?> GetByCedulaAsync(string cedula)
        {
            var c = await _repository.GetByIdAsync(cedula);
            if (c == null) return null;

            return new CiudadanoDto
            {
                Cedula = c.Cedula,
                Nombres = c.Nombre,
                Apellidos = c.Apellido,
                Telefono = c.Telefono,
                FechaNacimiento = c.Fecha_Nacimiento ?? DateTime.MinValue
            };
        }

        public async Task<CiudadanoDto> CreateAsync(CiudadanoDto dto)
        {
            var ciudadano = new Ciudadano
            {
                Cedula = dto.Cedula,
                Nombre = dto.Nombres,
                Apellido = dto.Apellidos,
                Telefono = dto.Telefono,
                Fecha_Nacimiento = dto.FechaNacimiento
            };

            await _repository.AddAsync(ciudadano);
            await _repository.SaveChangesAsync();
            return dto;
        }
    }
}
