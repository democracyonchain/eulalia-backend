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
    public class RolUsuarioController: ControllerBase
    {
        private readonly EulaliaContext _context;

        public RolUsuarioController(EulaliaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RolUsuario>>> GetAll()
        {
            return await _context.Roles.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RolUsuario>> GetById(int id)
        {
            var item = await _context.Roles.FindAsync(id);
            if (item == null) return NotFound();
            return item;
        }

        [HttpPost]
        public async Task<ActionResult<RolUsuario>> Create(RolUsuario rol)
        {
            _context.Roles.Add(rol);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = rol.Rol_Id }, rol);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, RolUsuario rol)
        {
            if (id != rol.Rol_Id) return BadRequest();
            _context.Entry(rol).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Roles.FindAsync(id);
            if (item == null) return NotFound();

            _context.Roles.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
