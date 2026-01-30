using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using eulalia_backend.Application.Interfaces;
using eulalia_backend.Application.DTOs;

namespace eulalia_backend.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SSIController : ControllerBase
    {
        private readonly ISSIService _service;

        public SSIController(ISSIService service)
        {
            _service = service;
        }

        [HttpPost("invitation/{cedula}")]
        public async Task<ActionResult<SSIInvitationDto>> RequestInvitation(string cedula)
        {
            var invitation = await _service.CreateInvitationAsync(cedula);
            return Ok(invitation);
        }

        [HttpGet("status/{cedula}")]
        public async Task<ActionResult<object>> GetStatus(string cedula)
        {
            var status = await _service.GetDidStatusAsync(cedula);
            return Ok(new { cedula, estado = status });
        }
    }
}
