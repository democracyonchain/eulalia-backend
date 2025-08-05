using eulalia_backend.Domain.EntitiesRequest;
using eulalia_backend.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eulalia_backend.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Validador")] // Solo estos roles pueden acceder
    public class BiometriaController : ControllerBase
    {
        private readonly BiometriaService _biometriaService;

        public BiometriaController(BiometriaService biometriaService)
        {
            _biometriaService = biometriaService;
        }

        /// <summary>
        /// Registrar datos biométricos (cifrado AES + hash)
        /// </summary>
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> RegistrarBiometria([FromForm] RegistrarBiometriaRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Cedula) || request.TemplateFile == null)
                return BadRequest(new { mensaje = "Debe enviar cédula y archivo biométrico." });

            using var ms = new MemoryStream();
            await request.TemplateFile.CopyToAsync(ms);
            var templateBytes = ms.ToArray();

            try
            {
                await _biometriaService.RegistrarBiometriaAsync(request.Cedula, templateBytes);
                return Ok(new { mensaje = "Biometría registrada correctamente." });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }


        /// <summary>
        /// Consultar estado de verificación biométrica
        /// </summary>
        [HttpGet("{cedula}")]
        public async Task<IActionResult> ObtenerEstado(string cedula)
        {
            var biometria = await _biometriaService.ObtenerPorCedulaAsync(cedula);
            if (biometria == null) return NotFound(new { mensaje = "Registro no encontrado." });

            return Ok(new
            {
                cedula = biometria.Cedula,
                estado = biometria.Estadoverificacion,
                fechaRegistro = biometria.Fecharegistro
            });
        }

        /// <summary>
        /// Actualizar estado de verificación
        /// </summary>
        [HttpPut("{cedula}/estado")]
        public async Task<IActionResult> ActualizarEstado(string cedula, [FromBody] string nuevoEstado)
        {
            if (string.IsNullOrWhiteSpace(nuevoEstado))
                return BadRequest(new { mensaje = "Debe enviar un estado válido." });

            try
            {
                await _biometriaService.ActualizarEstadoAsync(cedula, nuevoEstado);
                return Ok(new { mensaje = "Estado actualizado correctamente." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { mensaje = "Registro no encontrado." });
            }
        }
    }
}
