using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eulalia_backend.Domain.Entities
{
    [Table("rolusuario")]
    public class RolUsuario
    {
        [Key]
        public int Rol_Id { get; set; }
        public string Nombre { get; set; } = default!;
    }
}
