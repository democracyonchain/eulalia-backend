using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eulalia_backend.Domain.Entities
{
    public class BiometriaCiudadano
    {
        [Key]
        public int Biometriaid { get; set; }

        [Required]
        [MaxLength(50)]
        public string Cedula { get; set; } = string.Empty;

        [Required]
        public byte[] Templatecifrado { get; set; }

        [Required]
        [MaxLength(256)]
        public string Hashtemplate { get; set; } = string.Empty;

        [Required]
        public DateTime Fecharegistro { get; set; }

        [Required]
        [MaxLength(20)]
        public string Estadoverificacion { get; set; } = "pendiente";

        [ForeignKey("Cedula")]
        public Ciudadano Ciudadano { get; set; }
    }
}
