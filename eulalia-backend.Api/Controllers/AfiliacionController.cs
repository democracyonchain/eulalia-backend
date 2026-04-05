using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using eulalia_backend.Application.Interfaces;
using eulalia_backend.Application.DTOs;
using eulalia_backend.Domain.Enums;
using eulalia_backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eulalia_backend.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AfiliacionController : ControllerBase
    {
        private readonly IAfiliacionService _service;
        private readonly EulaliaContext _context;

        public AfiliacionController(IAfiliacionService service, EulaliaContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AfiliacionDto>>> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AfiliacionDto>> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<AfiliacionDto>> Create(AfiliacionDto dto)
        {
            try
            {
                var ciudadanoExiste = await _context.Ciudadanos
                    .AsNoTracking()
                    .AnyAsync(c => c.Cedula == dto.Cedula);
                if (!ciudadanoExiste)
                    return BadRequest(new { message = "No existe un ciudadano registrado con la cédula indicada." });

                var ultimaSsi = await _context.SsiIssuances
                    .AsNoTracking()
                    .Where(s => s.Cedula == dto.Cedula)
                    .OrderByDescending(s => s.UpdatedAt)
                    .FirstOrDefaultAsync();

                var ssiOk = ultimaSsi is not null &&
                            (ultimaSsi.Status == SsiIssuanceStatus.InvitationGenerated ||
                             ultimaSsi.Status == SsiIssuanceStatus.CredentialIssued);
                if (!ssiOk)
                    return BadRequest(new { message = "Debe completar la vinculación SSI antes de afiliarse." });

                var biometria = await _context.BiometriasCiudadano
                    .AsNoTracking()
                    .Where(b => b.Cedula == dto.Cedula)
                    .OrderByDescending(b => b.Fecharegistro)
                    .FirstOrDefaultAsync();

                var bioOk = biometria is not null &&
                            (string.Equals(biometria.Estadoverificacion, "enrolled", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(biometria.Estadoverificacion, "verificado", StringComparison.OrdinalIgnoreCase));
                if (!bioOk)
                    return BadRequest(new { message = "Debe completar la biometría antes de afiliarse." });

                var afiliacionActiva = await _context.Afiliaciones
                    .AsNoTracking()
                    .AnyAsync(a => a.Cedula == dto.Cedula && a.Estado != "Anulado");
                if (afiliacionActiva)
                    return Conflict(new { message = "El ciudadano ya tiene una afiliación activa." });

                var created = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.AfiliacionId }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/anular")]
        public async Task<IActionResult> AnularAfiliacion(int id)
        {
            var success = await _service.AnularAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
