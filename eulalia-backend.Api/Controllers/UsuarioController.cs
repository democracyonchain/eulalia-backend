using eulalia_backend.Api.Utils;
using eulalia_backend.Domain.Entities;
using eulalia_backend.Domain.EntitiesRequest;
using eulalia_backend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;


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

        [HttpPost("crear-con-ciudadano")]
        public async Task<ActionResult<Usuario>> CreateWithCitizen(UsuarioCreateRequest request)
        {
            try
            {
                // Verifica si el ciudadano ya existe
                var ciudadanoExistente = await _context.Ciudadanos.FindAsync(request.Cedula_Ciudadano);
                if (ciudadanoExistente == null)
                {
                    var nuevoCiudadano = new Ciudadano
                    {
                        Cedula = request.Cedula_Ciudadano,
                        Nombre = request.Nombre,
                        Apellido = request.Apellido,
                        Fecha_Nacimiento = DateTime.SpecifyKind(request.Fecha_Nacimiento, DateTimeKind.Utc),

                        Direccion = request.Direccion,
                        Telefono = request.Telefono
                    };

                    _context.Ciudadanos.Add(nuevoCiudadano);
                }

                // Crear usuario
                var nuevoUsuario = new Usuario
                {
                    Correo = request.Correo,
                    Contrasena = PasswordHelper.HashPassword(request.Contrasena),
                    Rol_Id = request.Rol_Id,
                    Cedula_Ciudadano = request.Cedula_Ciudadano,
                    Fecha_Creacion = DateTimeHelper.EnsureUtc(DateTime.UtcNow)
                };


                _context.Usuarios.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = nuevoUsuario.Usuario_Id }, nuevoUsuario);


            }
            catch (Exception ex)
            {
                return null;
            }

            }

        [HttpPost("crear-usuario-ciudadano")]
        [AllowAnonymous]
        public async Task<ActionResult<Usuario>> CreateUserCitizen(UsuarioCreateRequest request)
        {
            try
            {
                // Verifica si el ciudadano ya existe
                var ciudadanoExistente = await _context.Ciudadanos.FindAsync(request.Cedula_Ciudadano);
                if (ciudadanoExistente == null)
                {
                    var nuevoCiudadano = new Ciudadano
                    {
                        Cedula = request.Cedula_Ciudadano,
                        Nombre = request.Nombre,
                        Apellido = request.Apellido,
                        Fecha_Nacimiento = DateTime.SpecifyKind(request.Fecha_Nacimiento, DateTimeKind.Utc),

                        Direccion = request.Direccion,
                        Telefono = request.Telefono
                    };

                    _context.Ciudadanos.Add(nuevoCiudadano);
                }

                // Crear usuario
                var nuevoUsuario = new Usuario
                {
                    Correo = request.Correo,
                    Contrasena = PasswordHelper.HashPassword(request.Contrasena),
                    Rol_Id = request.Rol_Id,
                    Cedula_Ciudadano = request.Cedula_Ciudadano,
                    Fecha_Creacion = DateTimeHelper.EnsureUtc(DateTime.UtcNow)
                };


                _context.Usuarios.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = nuevoUsuario.Usuario_Id }, nuevoUsuario);


            }
            catch (Exception ex)
            {
                return null;
            }

        }


    }
}
