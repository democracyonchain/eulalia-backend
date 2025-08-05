using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace eulalia_backend.Domain.Entities
{
    [Table("organizacion")]
    public class Organizacion
    {
        [Key]
        public int Organizacion_Id { get; set; }
        public string Nombre { get; set; } = default!;
        public string Tipo_Organizacion { get; set; } = default!;
        public int? Codigo_Provincia { get; set; }
        public int? Codigo_Canton { get; set; }
        public int? Codigo_Parroquia { get; set; }
        public string Responsable_Cedula { get; set; } = default!;
        public string Estado_Validacion { get; set; } = "pendiente";
        public DateTime Fecha_Creacion { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(Responsable_Cedula))]
        public Ciudadano? Responsable { get; set; }
    }
}
