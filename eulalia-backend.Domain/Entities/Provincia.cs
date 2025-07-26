using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eulalia_backend.Domain.Entities
{
    [Table("provincia")]
    public class Provincia
    {
        [Key]
        public int Codigo_Provincia { get; set; }
        public string Nombre { get; set; } = default!;
    }
}
