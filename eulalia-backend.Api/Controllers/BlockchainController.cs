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
    public class BlockchainController : ControllerBase
    {
        private readonly EulaliaContext _context;

        public BlockchainController(EulaliaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Blockchain>>> GetAll()
        {
            return await _context.Blockchain.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Blockchain>> GetById(int id)
        {
            var item = await _context.Blockchain.FindAsync(id);
            if (item == null) return NotFound();
            return item;
        }

        [HttpPost]
        public async Task<ActionResult<Blockchain>> Create(Blockchain blockchain)
        {
            blockchain.FechaTransaccion = DateTimeHelper.EnsureUtc(blockchain.FechaTransaccion);
            _context.Blockchain.Add(blockchain);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = blockchain.BlockchainId }, blockchain);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Blockchain blockchain)
        {
            if (id != blockchain.BlockchainId) return BadRequest();
            blockchain.FechaTransaccion = DateTimeHelper.EnsureUtc(blockchain.FechaTransaccion);
            _context.Entry(blockchain).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Blockchain.FindAsync(id);
            if (item == null) return NotFound();

            _context.Blockchain.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent(); 
        }
    }
}
