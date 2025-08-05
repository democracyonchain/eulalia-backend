using eulalia_backend.Domain.Entities;
using eulalia_backend.Domain.EntitiesRequest;
using eulalia_backend.Infrastructure.Data;
using eulalia_backend.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eulalia_backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SolicitudOrganizacionController : ControllerBase
    {
        private readonly EulaliaContext _context;

        public SolicitudOrganizacionController(EulaliaContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Crear una nueva solicitud de organización (público)
        /// </summary>
        [HttpPost("crear")]
        [AllowAnonymous]
        public async Task<IActionResult> Crear([FromBody] SolicitudOrganizacionRequest data)
        {
            if (!ModelState.IsValid)
                return BadRequest("❌ Datos inválidos.");

            var organizacionExistente = await _context.Organizaciones
                .AnyAsync(o => o.Responsable_Cedula == data.Responsable_Cedula);
            if (organizacionExistente)
                return Conflict("⚠️ Ya existe una organización registrada con esta cédula.");

            var nombreExistente = await _context.Organizaciones
                .AnyAsync(o => o.Nombre.ToLower() == data.Nombre.ToLower());
            if (nombreExistente)
                return Conflict("⚠️ Ya existe una organización con ese nombre.");

            // Verificar existencia de ciudadano
            var ciudadano = await _context.Ciudadanos
                .FirstOrDefaultAsync(c => c.Cedula == data.Responsable_Cedula);

            if (ciudadano == null)
            {
                ciudadano = new Ciudadano
                {
                    Cedula = data.Responsable_Cedula,
                    Nombre = data.Responsable_Nombre,
                    Apellido = data.Responsable_Apellido,
                    Fecha_Nacimiento = DateTime.SpecifyKind(data.Responsable_FechaNacimiento, DateTimeKind.Utc),
                    Direccion = data.Responsable_Direccion,
                    Telefono = data.Responsable_Telefono
                };

                _context.Ciudadanos.Add(ciudadano);
                
                await _context.SaveChangesAsync();
               


            }

            // Verificar existencia de usuario
            var usuarioExistente = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == data.Responsable_Email);

            if (usuarioExistente != null)
                return Conflict("⚠️ Ya existe un usuario registrado con este correo.");

            // Crear usuario con rol Partido
            var usuario = new Usuario
            {
                Correo = data.Responsable_Email,
                Contrasena = PasswordHelper.HashPassword(data.Responsable_Cedula), 
                Cedula_Ciudadano = data.Responsable_Cedula,
                Rol_Id = 3, // Partido
                Fecha_Creacion = DateTime.UtcNow
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Crear organización
            var organizacion = new Organizacion
            {
                Nombre = data.Nombre,
                Tipo_Organizacion = data.Tipo_Organizacion,
                Codigo_Provincia = data.Codigo_Provincia,
                Codigo_Canton = data.Codigo_Canton,
                Codigo_Parroquia = data.Codigo_Parroquia,
                Responsable_Cedula = data.Responsable_Cedula,
                Estado_Validacion = "pendiente",
                Fecha_Creacion = DateTime.UtcNow
            };

            _context.Organizaciones.Add(organizacion);
            await _context.SaveChangesAsync();

            // Crear solicitud
            var solicitud = new SolicitudOrganizacion
            {
                Organizacion_Id = organizacion.Organizacion_Id,
                Estado = "pendiente",
                Observaciones = data.Observaciones,
                FechaSolicitud = DateTime.UtcNow
            };

            _context.SolicitudOrganizacion.Add(solicitud);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "✅ Solicitud registrada correctamente.",
                solicitudId = solicitud.Solicitud_Id
            });
        }

        [HttpGet("listar")]
        [Authorize]
        public async Task<IActionResult> Listar()
        {
            var solicitudes = await _context.SolicitudOrganizacion
                .Include(s => s.Organizacion)
                    .ThenInclude(o => o.Responsable) // ← esta línea es clave
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            return Ok(solicitudes);
        }


        /// <summary>
        /// Actualizar el estado de una solicitud (privado)
        /// </summary>
        [HttpPut("{id}/estado")]
        [Authorize]
        public async Task<IActionResult> ActualizarEstado(int id, [FromBody] UpdateEstadoSolicitudOrganizacionRequest request)
        {
            var solicitud = await _context.SolicitudOrganizacion
                .Include(s => s.Organizacion)
                .FirstOrDefaultAsync(s => s.Solicitud_Id == id);

            if (solicitud == null)
                return NotFound("❌ Solicitud no encontrada.");

            if (request.Estado != "aprobado" && request.Estado != "rechazado")
                return BadRequest("⚠️ Estado no válido. Debe ser 'aprobado' o 'rechazado'.");

            solicitud.Estado = request.Estado;
            solicitud.FechaRevision = DateTime.UtcNow;

            solicitud.Organizacion.Estado_Validacion = request.Estado;

            await _context.SaveChangesAsync();

            return Ok(new { message = "✅ Estado actualizado correctamente." });
        }

    }
}
