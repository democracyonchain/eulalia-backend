using eulalia_backend.Api.Utils;
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
    public class SSIController : ControllerBase
    {
        private readonly EulaliaContext _context;

        public SSIController(EulaliaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SSI>>> GetAll()
        {
            return await _context.SSIs.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SSI>> GetById(int id)
        {
            var item = await _context.SSIs.FindAsync(id);
            if (item == null) return NotFound();
            return item;
        }

        [HttpPost]
        public async Task<ActionResult<SSI>> Create(SSI ssi)
        {
            ssi.Fecha_Emision = DateTimeHelper.EnsureUtc(ssi.Fecha_Emision);
            _context.SSIs.Add(ssi);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = ssi.Ssi_Id }, ssi);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SSI ssi)
        {
            if (id != ssi.Ssi_Id) return BadRequest();
            ssi.Fecha_Emision = DateTimeHelper.EnsureUtc(ssi.Fecha_Emision);
            _context.Entry(ssi).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.SSIs.FindAsync(id);
            if (item == null) return NotFound();

            _context.SSIs.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
