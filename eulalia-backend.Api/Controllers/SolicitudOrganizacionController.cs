using eulalia_backend.Domain.Entities;
using eulalia_backend.Domain.EntitiesRequest;
using eulalia_backend.Infrastructure.Data;
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

            // Validar duplicados por cédula
            var organizacionExistente = await _context.Organizaciones
                .AnyAsync(o => o.Responsable_Cedula == data.Responsable_Cedula);
            if (organizacionExistente)
                return Conflict("⚠️ Ya existe una organización registrada con esta cédula.");

            // Validar duplicados por nombre
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
                    Fecha_Nacimiento = data.Responsable_FechaNacimiento,
                    Direccion = data.Responsable_Direccion,
                    Telefono = data.Responsable_Telefono
                };

                _context.Ciudadanos.Add(ciudadano);
                await _context.SaveChangesAsync();
            }

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

        /// <summary>
        /// Listar todas las solicitudes (solo admin y organismo)
        /// </summary>
        [HttpGet("listar")]
        [Authorize(Roles = "admin,organismo")]
        public async Task<IActionResult> Listar()
        {
            var solicitudes = await _context.SolicitudOrganizacion
                .Include(s => s.Organizacion)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            return Ok(solicitudes);
        }

        /// <summary>
        /// Cambiar el estado de una solicitud
        /// </summary>
        [HttpPut("actualizar-estado/{id}")]
        [Authorize(Roles = "admin,organismo")]
        public async Task<IActionResult> ActualizarEstado(int id, [FromBody] CambiarEstadoRequest request)
        {
            var solicitud = await _context.SolicitudOrganizacion.FindAsync(id);
            if (solicitud == null)
                return NotFound("❌ Solicitud no encontrada.");

            solicitud.Estado = request.Estado;
            solicitud.Observaciones = request.Observaciones;
            solicitud.FechaRevision = DateTime.UtcNow;
            solicitud.UsuarioRevisor = request.UsuarioRevisor;

            await _context.SaveChangesAsync();

            return Ok(new { message = "✅ Estado actualizado correctamente." });
        }

        public class CambiarEstadoRequest
        {
            public string Estado { get; set; } = "aprobada"; // o "rechazada"
            public string? Observaciones { get; set; }
            public int UsuarioRevisor { get; set; }
        }
    }
}
