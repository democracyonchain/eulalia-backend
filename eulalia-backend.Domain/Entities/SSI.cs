using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eulalia_backend.Domain.Entities
{
    [Table("ssi")]
    public class SSI
    {
        [Key]
        public int Ssi_Id { get; set; }
        public string Cedula { get; set; } = default!;
        public string Credencial_Digital { get; set; } = default!; // JSON serializado
        public string Estado { get; set; } = default!; // activa, revocada
        public DateTime Fecha_Emision { get; set; } = DateTime.UtcNow;
        public DateTime? Fecha_Revocacion { get; set; }
    }
}
