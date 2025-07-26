using eulalia_backend.Api.Utils;
using eulalia_backend.Domain.Entities;
using eulalia_backend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;


namespace eulalia_backend.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly EulaliaContext _context;

        public UsuarioController(EulaliaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetAll()
        {
            return await _context.Usuarios.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetById(int id)
        {
            var item = await _context.Usuarios.FindAsync(id);
            if (item == null) return NotFound();
            return item;
        }

        [HttpPost]
        public async Task<ActionResult<Usuario>> Create(Usuario usuario)
        {
            usuario.Fecha_Creacion = DateTimeHelper.EnsureUtc(usuario.Fecha_Creacion);
            usuario.Contrasena = PasswordHelper.HashPassword(usuario.Contrasena);
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = usuario.Usuario_Id }, usuario);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Usuario usuario)
        {
            if (id != usuario.Usuario_Id) return BadRequest();
            usuario.Fecha_Creacion = DateTimeHelper.EnsureUtc(usuario.Fecha_Creacion);
            _context.Entry(usuario).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Usuarios.FindAsync(id);
            if (item == null) return NotFound();

            _context.Usuarios.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        
    }
}
