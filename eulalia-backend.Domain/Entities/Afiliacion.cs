using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eulalia_backend.Domain.Entities
{
    [Table("afiliacion")] 
    public class Afiliacion
    {
        [Key]
        public int Afiliacion_Id { get; set; }
        public string Cedula { get; set; } = default!;
        public int OrganizacionId { get; set; }
        public DateTime FechaAfiliacion { get; set; } = DateTime.UtcNow;
        public string Estado { get; set; } = "activa"; // activa, cancelada
        public string? MotivoCancelacion { get; set; }
        public int? BlockchainTxId { get; set; }
        public string? CardanoAffiliationEventId { get; set; }
        public string? CardanoAffiliationRecordHash { get; set; }
        public string? CardanoAffiliationTxHash { get; set; }
        public string? CardanoAffiliationStatus { get; set; }
        public string? CardanoCancellationEventId { get; set; }
        public string? CardanoCancellationRecordHash { get; set; }
        public string? CardanoCancellationTxHash { get; set; }
        public string? CardanoCancellationStatus { get; set; }
        public int? UsuarioAprobador { get; set; }
        public bool EsUltima { get; set; } = true;
    }
}
