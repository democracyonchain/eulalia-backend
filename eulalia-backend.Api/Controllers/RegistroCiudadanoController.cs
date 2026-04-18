using eulalia_backend.Domain.Enums;
using eulalia_backend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eulalia_backend.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RegistroCiudadanoController : ControllerBase
    {
        private readonly EulaliaContext _context;

        public RegistroCiudadanoController(EulaliaContext context)
        {
            _context = context;
        }

        [HttpGet("estado/{cedula}")]
        public async Task<IActionResult> ObtenerEstado(string cedula)
        {
            var cedulaToken = User.FindFirst("cedula")?.Value;
            if (string.IsNullOrWhiteSpace(cedulaToken))
                return Forbid();

            if (!string.Equals(cedulaToken, cedula, StringComparison.Ordinal))
                return Forbid();

            var ciudadano = await _context.Ciudadanos
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Cedula == cedula);

            var ultimaSsi = await _context.SsiIssuances
                .AsNoTracking()
                .Where(s => s.Cedula == cedula)
                .OrderByDescending(s => s.UpdatedAt)
                .FirstOrDefaultAsync();

            var biometria = await _context.BiometriasCiudadano
                .AsNoTracking()
                .Where(b => b.Cedula == cedula)
                .OrderByDescending(b => b.Fecharegistro)
                .FirstOrDefaultAsync();

            var baseOk = ciudadano != null;
            var ssiOk = ultimaSsi is not null &&
                        ultimaSsi.Status == SsiIssuanceStatus.CredentialIssued;

            var bioOk = biometria != null &&
                        (string.Equals(biometria.Estadoverificacion, "enrolled", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(biometria.Estadoverificacion, "verificado", StringComparison.OrdinalIgnoreCase));

            return Ok(new
            {
                cedula,
                base_ok = baseOk,
                ssi_ok = ssiOk,
                bio_ok = bioOk,
                ready_for_affiliation = baseOk && ssiOk && bioOk,
                ssi_status = ultimaSsi?.Status.ToString() ?? "NotStarted",
                bio_status = biometria?.Estadoverificacion ?? "not_started",
                updated_at = DateTime.UtcNow
            });
        }
    }
}
