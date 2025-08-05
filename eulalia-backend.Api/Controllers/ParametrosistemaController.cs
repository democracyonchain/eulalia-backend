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
    public class ParametrosistemaController : ControllerBase
    {
        private readonly EulaliaContext _context;

        public ParametrosistemaController(EulaliaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Parametrosistema>>> GetAll()
        {
            return await _context.Parametrosistema.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Parametrosistema>> GetById(string id)
        {
            var param = await _context.Parametrosistema.FindAsync(id);
            if (param == null) return NotFound();
            return param;
        }

        [HttpPost]
        public async Task<ActionResult<Parametrosistema>> Create(Parametrosistema parametro)
        {
            _context.Parametrosistema.Add(parametro);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = parametro.ParametroId }, parametro);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, Parametrosistema parametro)
        {
            if (id != parametro.ParametroId) return BadRequest();

            _context.Entry(parametro).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var parametro = await _context.Parametrosistema.FindAsync(id);
            if (parametro == null) return NotFound();

            _context.Parametrosistema.Remove(parametro);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
