using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eulalia_backend.Infrastructure.Data;
using eulalia_backend.Domain.Entities;
using Microsoft.AspNetCore.Authorization;


namespace eulalia_backend.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CiudadanoController : ControllerBase
    {
        private readonly EulaliaContext _context;

        public CiudadanoController(EulaliaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ciudadano>>> GetAll()
        {
            return await _context.Ciudadanos.ToListAsync();
        }

        [HttpGet("{cedula}")]
        public async Task<ActionResult<Ciudadano>> GetByCedula(string cedula)
        {
            var ciudadano = await _context.Ciudadanos.FindAsync(cedula);
            if (ciudadano == null) return NotFound();
            return ciudadano;
        }

        [HttpPost]
        public async Task<ActionResult<Ciudadano>> Create(Ciudadano ciudadano)
        {
            if (ciudadano.Fecha_Nacimiento.HasValue)
                ciudadano.Fecha_Nacimiento = DateTime.SpecifyKind(ciudadano.Fecha_Nacimiento.Value, DateTimeKind.Utc);

            _context.Ciudadanos.Add(ciudadano);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetByCedula), new { cedula = ciudadano.Cedula }, ciudadano);

        }
    }
}
