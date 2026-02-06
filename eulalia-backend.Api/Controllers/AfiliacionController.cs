using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using eulalia_backend.Application.Interfaces;
using eulalia_backend.Application.DTOs;

namespace eulalia_backend.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AfiliacionController : ControllerBase
    {
        private readonly IAfiliacionService _service;

        public AfiliacionController(IAfiliacionService service)
        {
            _service = service;
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
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.AfiliacionId }, created);
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
