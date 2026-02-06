using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using eulalia_backend.Application.Interfaces;
using eulalia_backend.Application.DTOs;

namespace eulalia_backend.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CiudadanoController : ControllerBase
    {
        private readonly ICiudadanoService _service;

        public CiudadanoController(ICiudadanoService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CiudadanoDto>>> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{cedula}")]
        public async Task<ActionResult<CiudadanoDto>> GetByCedula(string cedula)
        {
            var item = await _service.GetByCedulaAsync(cedula);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<CiudadanoDto>> Create(CiudadanoDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetByCedula), new { cedula = created.Cedula }, created);
        }
    }
}
