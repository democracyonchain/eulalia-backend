using eulalia_backend.Domain.Entities;
using eulalia_backend.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;


namespace eulalia_backend.Api.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class ProvinciaController : ControllerBase
    {
        private readonly EulaliaContext _context;

        public ProvinciaController(EulaliaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Provincia>>> GetAll()
        {
            return await _context.Provincias.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Provincia>> GetById(int id)
        {
            var provincia = await _context.Provincias.FindAsync(id);
            if (provincia == null) return NotFound();
            return provincia;
        }

        [HttpPost]
        public async Task<ActionResult<Provincia>> Create(Provincia provincia)
        {
            _context.Provincias.Add(provincia);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = provincia.Codigo_Provincia }, provincia);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Provincia provincia)
        {
            if (id != provincia.Codigo_Provincia) return BadRequest();
            _context.Entry(provincia).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var provincia = await _context.Provincias.FindAsync(id);
            if (provincia == null) return NotFound();

            _context.Provincias.Remove(provincia);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
