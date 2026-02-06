using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using eulalia_backend.Application.Interfaces;
using eulalia_backend.Application.DTOs;

namespace eulalia_backend.Api.Controllers
{
    
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProvinciaController : ControllerBase
    {
        private readonly IProvinciaService _service;

        public ProvinciaController(IProvinciaService service)
        {
            _service = service;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProvinciaDto>>> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProvinciaDto>> GetById(int id)
        {
            var provincia = await _service.GetByIdAsync(id);
            if (provincia == null) return NotFound();
            return Ok(provincia);
        }

        [HttpPost]
        public async Task<ActionResult<ProvinciaDto>> Create(ProvinciaDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Codigo }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ProvinciaDto dto)
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
}
