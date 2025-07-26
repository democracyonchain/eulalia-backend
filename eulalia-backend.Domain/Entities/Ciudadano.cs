using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eulalia_backend.Domain.Entities
{
    [Table("ciudadano")]
    public class Ciudadano
    {
        [Key] 
        public string Cedula { get; set; } = default!;
        public string Nombre { get; set; } = default!;
        public string Apellido { get; set; } = default!;
        public DateTime? Fecha_Nacimiento { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
    }
}
