using eulalia_backend.Api.Models;
using eulalia_backend.Api.Settings;
using eulalia_backend.Api.Utils;
using eulalia_backend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace eulalia_backend.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthSettings _authSettings;
        private readonly EulaliaContext _context;

        public AuthController(IOptions<AuthSettings> authSettings, EulaliaContext context)
        {
            _authSettings = authSettings.Value;
            _context = context;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == request.email);

            if (usuario == null || !PasswordHelper.VerifyPassword(request.Password, usuario.Contrasena))
                return Unauthorized("Correo o contraseña inválidos");

            if (usuario == null)
                return Unauthorized("Correo o contraseña inválidos");

            var token = GenerateJwtToken(usuario.Correo, usuario.Rol_Id);
            return Ok(new { token });
        }



        private string GenerateJwtToken(string correo, int rolId)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, correo),
                new Claim("rol", rolId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _authSettings.Issuer,
                audience: _authSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_authSettings.ExpirationMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

   
}
