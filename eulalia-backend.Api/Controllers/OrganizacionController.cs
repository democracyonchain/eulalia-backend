using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using eulalia_backend.Application.Interfaces;
using eulalia_backend.Application.DTOs;

namespace eulalia_backend.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizacionController : ControllerBase
    {
        private readonly IOrganizacionService _service;

        public OrganizacionController(IOrganizacionService service)
        {
            _service = service;
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
}
