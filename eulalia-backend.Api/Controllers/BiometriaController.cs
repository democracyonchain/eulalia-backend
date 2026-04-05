using eulalia_backend.Domain.EntitiesRequest;
using eulalia_backend.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace eulalia_backend.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BiometriaController : ControllerBase
    {
        private readonly BiometriaService _biometriaService;
        private readonly double _livenessMinScore;
        private readonly int _maxEmbeddingLength;

        public BiometriaController(BiometriaService biometriaService, IConfiguration configuration)
        {
            _biometriaService = biometriaService;
            _livenessMinScore = configuration.GetValue<double?>("Biometria:LivenessMinScore") ?? 0.75;
            _maxEmbeddingLength = configuration.GetValue<int?>("Biometria:MaxEmbeddingBase64Length") ?? 16384;
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

        [HttpPost("enroll-self")]
        public async Task<IActionResult> RegistrarBiometriaSelf([FromBody] RegistrarBiometriaSelfRequest request)
        {
            var cedulaToken = User.FindFirst("cedula")?.Value;
            if (string.IsNullOrWhiteSpace(cedulaToken))
                return Forbid();

            if (!string.Equals(cedulaToken, request.Cedula, StringComparison.Ordinal))
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.LivenessScore < _livenessMinScore)
                return BadRequest(new
                {
                    mensaje = $"Liveness insuficiente. Mínimo requerido: {_livenessMinScore:F2}.",
                    score = request.LivenessScore
                });

            if (request.EmbeddingBase64.Length > _maxEmbeddingLength)
                return BadRequest(new { mensaje = "EmbeddingBase64 excede el tamaño permitido." });

            try
            {
                byte[] templateBytes;
                try
                {
                    templateBytes = Convert.FromBase64String(request.EmbeddingBase64);
                }
                catch (FormatException)
                {
                    return BadRequest(new { mensaje = "EmbeddingBase64 no tiene un formato válido." });
                }

                await _biometriaService.RegistrarBiometriaAsync(request.Cedula, templateBytes, "enrolled");

                return Ok(new
                {
                    mensaje = "Biometría registrada correctamente.",
                    cedula = request.Cedula,
                    estado = "enrolled",
                    livenessScore = request.LivenessScore,
                    modelVersion = request.ModelVersion
                });
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
