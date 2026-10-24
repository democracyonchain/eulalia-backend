using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using eulalia_backend.Application.Interfaces;
using eulalia_backend.Application.DTOs;
using eulalia_backend.Domain.Entities;

namespace eulalia_backend.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizacionController : ControllerBase
    {
        private readonly IOrganizacionService _service;
        private readonly IOrganizacionIdentityService? _identityService;
        private readonly IRepository<OrganizacionIdentidad>? _identidades;
        private readonly IIdentusClient? _identus;

        public OrganizacionController(
            IOrganizacionService service,
            IRepository<OrganizacionIdentidad>? identidades = null,
            IIdentusClient? identus = null,
            IOrganizacionIdentityService? identityService = null)
        {
            _service = service;
            _identityService = identityService;
            _identidades = identidades;
            _identus = identus;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrganizacionDto>>> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrganizacionDto>> GetById(int id)
        {
            var org = await _service.GetByIdAsync(id);
            if (org == null) return NotFound();
            return Ok(org);
        }

        [HttpGet("identidad")]
        public async Task<ActionResult<OrganizacionIdentidadDto>> GetIdentidad()
        {
            var claim = User.FindFirstValue("organizacion_id");
            if (!int.TryParse(claim, out var organizacionId))
            {
                return Forbid();
            }

            var org = await _service.GetByIdAsync(organizacionId);
            if (org == null) return NotFound();
            var identity = _identidades == null
                ? null
                : (await _identidades.FindAsync(i => i.OrganizacionId == organizacionId)).SingleOrDefault();

            if (_identityService != null)
                identity = await _identityService.RefreshCredentialStatusAsync(organizacionId, HttpContext.RequestAborted);

            return Ok(new OrganizacionIdentidadDto
            {
                OrganizacionId = org.OrganizacionId,
                Nombre = org.Nombre,
                EstadoAprobacion = org.Estado,
                Did = identity?.ClientDid ?? identity?.PublishedDid ?? identity?.LongFormDid,
                 ClientDid = identity?.ClientDid,
                 HolderConnectionId = identity?.HolderConnectionId,
                LongFormDid = identity?.LongFormDid,
                PublishedDid = identity?.PublishedDid,
                EstadoIdentidad = identity?.Estado,
                EstadoCredencial = identity?.CredentialStatus,
                ReferenciaEmisor = identity?.IssuerReference,
                ReferenciaOperacion = identity?.PublicationOperationId,
                CredentialRecordId = identity?.CredentialRecordId,
                CreadoEn = identity?.CreatedAt,
                ActualizadoEn = identity?.UpdatedAt,
                 Error = identity?.ErrorMessage is { Length: > 1000 } error ? error[..1000] : identity?.ErrorMessage
            });
        }

        [HttpPut("identidad/did")]
        public async Task<IActionResult> AsociarDid([FromBody] ClientDidRequest request)
        {
            var claim = User.FindFirstValue("organizacion_id");
            if (!int.TryParse(claim, out var organizacionId)) return Forbid();
            if (User.FindFirstValue("rol") is not ("2" or "3")) return Forbid();
            if (!DidValidator.IsValid(request.Did)) return BadRequest(new { message = "El DID no tiene un formato válido." });
            if (_identidades == null) return StatusCode(StatusCodes.Status503ServiceUnavailable);

            var org = await _service.GetByIdAsync(organizacionId);
            if (org == null) return NotFound();
            var identity = (await _identidades.FindAsync(i => i.OrganizacionId == organizacionId)).SingleOrDefault();
            if (identity == null)
            {
                identity = new OrganizacionIdentidad { OrganizacionId = organizacionId, Estado = "wallet_connected" };
                await _identidades.AddAsync(identity);
            }
            var hasCredential = !string.IsNullOrWhiteSpace(identity.CredentialRecordId) ||
                identity.CredentialStatus is "accepted" or "issued" or "completed";
            if (identity.ClientDid is not null &&
                (!string.Equals(org.Estado, "pendiente", StringComparison.OrdinalIgnoreCase) || hasCredential))
                return Conflict(new { message = "La organización ya tiene otro DID asociado." });

            identity.ClientDid = request.Did;
            identity.Estado = "wallet_connected";
            identity.ErrorMessage = null;
            identity.UpdatedAt = DateTime.UtcNow;
            await _identidades.SaveChangesAsync();
            return Ok(new { did = identity.ClientDid, status = identity.Estado });
        }

        [HttpPost("identidad/invitacion")]
        public async Task<IActionResult> CrearInvitacionHolder()
        {
            if (User.FindFirstValue("organizacion_id") is not string claim ||
                !int.TryParse(claim, out _) ||
                User.FindFirstValue("rol") is not ("2" or "3"))
                return Forbid();
            if (_identus is null) return StatusCode(StatusCodes.Status503ServiceUnavailable);

            try
            {
                var invitation = await _identus.CreateInvitationAsync("Eulalia organization holder");
                return Ok(new { invitationUrl = invitation.InvitationUrl, invitationId = invitation.InvitationId });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Identus no pudo crear la invitación del holder." });
            }
        }

        [HttpPost("identidad/credencial/reintentar")]
        public async Task<IActionResult> ReintentarCredencial()
        {
            if (User.FindFirstValue("organizacion_id") is not string claim ||
                !int.TryParse(claim, out var organizacionId) ||
                User.FindFirstValue("rol") is not ("2" or "3"))
                return Forbid();
            if (_identityService is null) return StatusCode(StatusCodes.Status503ServiceUnavailable);

            try
            {
                var identity = await _identityService.RetryOrganizationCredentialAsync(organizacionId, HttpContext.RequestAborted);
                return Ok(new
                {
                    status = identity.Estado,
                    credentialStatus = identity.CredentialStatus,
                    credentialRecordId = identity.CredentialRecordId,
                    message = identity.ErrorMessage
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Identus no pudo crear una nueva oferta de credencial." });
            }
        }

        [HttpPost("identidad/credencial/confirmar-recepcion")]
        public async Task<IActionResult> ConfirmarRecepcionCredencial()
        {
            if (User.FindFirstValue("organizacion_id") is not string claim ||
                !int.TryParse(claim, out var organizacionId) ||
                User.FindFirstValue("rol") is not ("2" or "3"))
                return Forbid();
            if (_identityService is null) return StatusCode(StatusCodes.Status503ServiceUnavailable);

            try
            {
                var identity = await _identityService.ConfirmCredentialReceiptAsync(organizacionId, HttpContext.RequestAborted);
                return Ok(new
                {
                    status = identity.Estado,
                    credentialStatus = identity.CredentialStatus,
                    message = identity.ErrorMessage
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPost("identidad/conexion/recuperar")]
        public async Task<IActionResult> RecuperarConexionHolder()
        {
            var claim = User.FindFirstValue("organizacion_id");
            if (!int.TryParse(claim, out var organizacionId) || User.FindFirstValue("rol") is not ("2" or "3"))
                return Forbid();
            if (_identidades == null || _identus == null)
                return StatusCode(StatusCodes.Status503ServiceUnavailable);

            var identity = (await _identidades.FindAsync(i => i.OrganizacionId == organizacionId)).SingleOrDefault();
            if (identity == null || string.IsNullOrWhiteSpace(identity.ClientDid))
                return Conflict(new { message = "La identidad de la organización todavía no tiene un DID de wallet asociado." });
            if (identity.Estado == "completed" || identity.CredentialStatus is "accepted" or "issued" or "completed")
                return Conflict(new { message = "La credencial de la organización ya fue aceptada." });

            if (!string.IsNullOrWhiteSpace(identity.HolderConnectionId))
            {
                var connection = await _identus.GetConnectionStatusAsync(identity.HolderConnectionId, HttpContext.RequestAborted);
                if (connection is not null && IsEstablishedForWallet(connection))
                    return Ok(new { connected = true, holderConnectionId = identity.HolderConnectionId });
            }

            try
            {
                var invitation = await _identus.CreateInvitationAsync("Eulalia organization holder recovery");
                // Persist only the newly issued invitation after Identus accepted its creation.
                identity.HolderConnectionId = invitation.InvitationId;
                identity.UpdatedAt = DateTime.UtcNow;
                await _identidades.SaveChangesAsync();
                return Ok(new
                {
                    connected = false,
                    invitationUrl = invitation.InvitationUrl,
                    invitationId = invitation.InvitationId,
                    holderConnectionId = identity.HolderConnectionId
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Identus no pudo recuperar la conexión del holder." });
            }
        }

        [HttpGet("identidad/conexion/estado")]
        public async Task<IActionResult> EstadoConexionHolder()
        {
            var claim = User.FindFirstValue("organizacion_id");
            if (!int.TryParse(claim, out var organizacionId) || User.FindFirstValue("rol") is not ("2" or "3"))
                return Forbid();
            if (_identidades == null || _identus == null)
                return StatusCode(StatusCodes.Status503ServiceUnavailable);

            var identity = (await _identidades.FindAsync(i => i.OrganizacionId == organizacionId)).SingleOrDefault();
            if (identity == null || string.IsNullOrWhiteSpace(identity.ClientDid) || string.IsNullOrWhiteSpace(identity.HolderConnectionId))
                return Ok(new { connected = false, holderConnectionId = identity?.HolderConnectionId });

            var connection = await _identus.GetConnectionStatusAsync(identity.HolderConnectionId, HttpContext.RequestAborted);
            return Ok(new
            {
                connected = connection is not null && IsEstablishedForWallet(connection),
                holderConnectionId = identity.HolderConnectionId
            });
        }

        [HttpPut("identidad/conexion")]
        public async Task<IActionResult> AsociarConexion([FromBody] HolderConnectionRequest request)
        {
            var claim = User.FindFirstValue("organizacion_id");
            if (!int.TryParse(claim, out var organizacionId)) return Forbid();
            if (User.FindFirstValue("rol") is not ("2" or "3")) return Forbid();
            if (!HolderConnectionValidator.IsValid(request.HolderConnectionId))
                return BadRequest(new { message = "La referencia de conexión no tiene un formato válido." });
            if (_identidades == null) return StatusCode(StatusCodes.Status503ServiceUnavailable);

            var org = await _service.GetByIdAsync(organizacionId);
            if (org == null) return NotFound();
            if (!string.Equals(org.Estado, "pendiente", StringComparison.OrdinalIgnoreCase))
                return Conflict(new { message = "La conexión solo puede asociarse mientras la solicitud está pendiente." });

            var identity = (await _identidades.FindAsync(i => i.OrganizacionId == organizacionId)).SingleOrDefault();
            if (identity == null)
            {
                identity = new OrganizacionIdentidad { OrganizacionId = organizacionId, Estado = "wallet_connected" };
                await _identidades.AddAsync(identity);
            }

            if (identity.CredentialStatus is "accepted" or "issued" or "completed")
                return Conflict(new { message = "La conexión no puede reemplazarse después de iniciar o completar la emisión." });

            identity.HolderConnectionId = request.HolderConnectionId.Trim();
            identity.UpdatedAt = DateTime.UtcNow;
            await _identidades.SaveChangesAsync();
            return Ok(new { holderConnectionId = identity.HolderConnectionId });
        }

        private static bool IsEstablishedForWallet(IdentusConnectionStatus connection) =>
            (string.Equals(connection.State, "ConnectionResponseSent", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(connection.State, "ConnectionResponseReceived", StringComparison.OrdinalIgnoreCase));

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<OrganizacionDto>> Create(OrganizacionDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.OrganizacionId }, created);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, OrganizacionDto dto)
        {
            var success = await _service.UpdateAsync(id, dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }

    public sealed class ClientDidRequest
    {
        public string Did { get; set; } = string.Empty;
    }

    public sealed class HolderConnectionRequest
    {
        public string HolderConnectionId { get; set; } = string.Empty;
    }

        internal static class DidValidator
    {
        private static readonly System.Text.RegularExpressions.Regex Pattern = new(
            "^did:(peer|prism):[^\\s]+$",
            System.Text.RegularExpressions.RegexOptions.Compiled);

        public static bool IsValid(string? did) =>
            !string.IsNullOrWhiteSpace(did) &&
            did.Length <= 4096 &&
            !did.Contains("undefined", StringComparison.Ordinal) &&
            Pattern.IsMatch(did);
    }

    internal static class HolderConnectionValidator
    {
        private static readonly System.Text.RegularExpressions.Regex Pattern = new(
            "^[A-Za-z0-9][A-Za-z0-9._:-]{0,511}$",
            System.Text.RegularExpressions.RegexOptions.Compiled);

        public static bool IsValid(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || !Pattern.IsMatch(value.Trim())) return false;
            var lower = value.Trim().ToLowerInvariant();
            return !lower.Contains("private") && !lower.Contains("seed") && !lower.Contains("mnemonic");
        }
    }
}
