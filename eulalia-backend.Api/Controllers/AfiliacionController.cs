
using eulalia_backend.Domain.Entities;
using eulalia_backend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eulalia_backend.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AfiliacionController : ControllerBase
    {
        private readonly EulaliaContext _context;

        public AfiliacionController(EulaliaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Afiliacion>>> GetAll()
        {
            return await _context.Afiliaciones.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Afiliacion>> GetById(int id)
        {
            var item = await _context.Afiliaciones.FindAsync(id);
            if (item == null) return NotFound();
            return item;
        }

        [HttpPost]
        public async Task<ActionResult<Afiliacion>> Create(Afiliacion afiliacion)
        {
            if (afiliacion.FechaAfiliacion.Kind == DateTimeKind.Unspecified)
                afiliacion.FechaAfiliacion = DateTime.SpecifyKind(afiliacion.FechaAfiliacion, DateTimeKind.Utc);

            _context.Afiliaciones.Add(afiliacion);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = afiliacion.Afiliacion_Id }, afiliacion);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Afiliacion afiliacion)
        {
            if (id != afiliacion.Afiliacion_Id) return BadRequest();
            _context.Entry(afiliacion).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("afiliados/{organizacionId}")]
        public async Task<IActionResult> GetAfiliadosPorOrganizacion(int organizacionId)
        {
            var afiliados = await _context.Afiliaciones
                .Where(a => a.OrganizacionId == organizacionId && a.Estado == "activo")
                .Select(a => new
                {
                    a.Afiliacion_Id,
                    a.Cedula,
                    a.FechaAfiliacion,
                    a.Estado
                })
                .ToListAsync();

            return Ok(afiliados);
        }
        [HttpPut("{id}/anular")]
        public async Task<IActionResult> AnularAfiliacion(int id)
        {
            var afiliacion = await _context.Afiliaciones.FindAsync(id);
            if (afiliacion == null)
                return NotFound();

            afiliacion.Estado = "Anulado";
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Afiliaciones.FindAsync(id);
            if (item == null) return NotFound();

            _context.Afiliaciones.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
