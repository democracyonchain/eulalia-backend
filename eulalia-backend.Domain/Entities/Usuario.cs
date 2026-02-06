using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eulalia_backend.Domain.Entities
{
    [Table("usuario")]
    public class Usuario
    {
        [Key]
        public int Usuario_Id { get; set; }
        public string Correo { get; set; } = default!;
        public string Contrasena { get; set; } = default!;
        public int Rol_Id { get; set; }
        public string? Cedula_Ciudadano { get; set; }
        public DateTime? Fecha_Creacion { get; set; }

        [ForeignKey(nameof(Cedula_Ciudadano))]
        public virtual Ciudadano? Ciudadano { get; set; }
    }
}

