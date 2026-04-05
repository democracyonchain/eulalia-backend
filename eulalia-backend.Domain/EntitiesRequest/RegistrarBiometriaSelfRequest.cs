using System.ComponentModel.DataAnnotations;

namespace eulalia_backend.Domain.EntitiesRequest
{
    public class RegistrarBiometriaSelfRequest
    {
        [Required]
        public string Cedula { get; set; } = string.Empty;

        [Required]
        public string EmbeddingBase64 { get; set; } = string.Empty;

        [Range(0, 1)]
        public double LivenessScore { get; set; }

        [MaxLength(50)]
        public string? ModelVersion { get; set; }
    }
}
