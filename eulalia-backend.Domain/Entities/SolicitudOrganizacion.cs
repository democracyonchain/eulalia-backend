using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eulalia_backend.Domain.Entities
{
    [Table("solicitudorganizacion")]
    public class SolicitudOrganizacion
    {
        [Key]
        public int Solicitud_Id { get; set; }
        public int Organizacion_Id { get; set; }
        public string Estado { get; set; } = "pendiente";
        public string? Observaciones { get; set; }
        public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;
        public DateTime? FechaRevision { get; set; }
        public int? UsuarioRevisor { get; set; }
        [ForeignKey("Organizacion_Id")]
        public Organizacion Organizacion { get; set; } = default!;
    }
}
