using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eulalia_backend.Domain.Entities
{
    [Table("auditoria")]
    public class Auditoria
    {
        public int AuditoriaId { get; set; }
        public string Accion { get; set; } = default!;
        public int UsuarioId { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public string? Descripcion { get; set; }
    }
}
