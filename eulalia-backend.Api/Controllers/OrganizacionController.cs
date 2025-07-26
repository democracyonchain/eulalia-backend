using eulalia_backend.Domain.Entities;
using eulalia_backend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eulalia_backend.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizacionController : ControllerBase
    {
        private readonly EulaliaContext _context;

        public OrganizacionController(EulaliaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Organizacion>>> GetAll()
        {
            return await _context.Organizaciones.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Organizacion>> GetById(int id)
        {
            var org = await _context.Organizaciones.FindAsync(id);
            if (org == null) return NotFound();
            return org;
        }

        [HttpPost]
        public async Task<ActionResult<Organizacion>> Create(Organizacion organizacion)
        {

            if (organizacion.Fecha_Creacion.Kind == DateTimeKind.Unspecified)
                organizacion.Fecha_Creacion = DateTime.SpecifyKind(organizacion.Fecha_Creacion, DateTimeKind.Utc);


            _context.Organizaciones.Add(organizacion);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = organizacion.Organizacion_Id }, organizacion);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Organizacion organizacion)
        {
            if (id != organizacion.Organizacion_Id) return BadRequest();

            _context.Entry(organizacion).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var organizacion = await _context.Organizaciones.FindAsync(id);
            if (organizacion == null) return NotFound();

            _context.Organizaciones.Remove(organizacion);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
