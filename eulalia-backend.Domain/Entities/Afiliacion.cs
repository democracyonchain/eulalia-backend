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
        public int? UsuarioAprobador { get; set; }
        public bool EsUltima { get; set; } = true;
    }
}
