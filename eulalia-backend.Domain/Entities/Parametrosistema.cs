using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace eulalia_backend.Domain.Entities
{
    [Table("parametrosistema")]
    public class Parametrosistema
    {
        [Key]
        [Column("parametro_id")]
        public string ParametroId { get; set; } = null!;

        [Column("valor")]
        public string Valor { get; set; } = null!;

        [Column("tipo")]
        public string Tipo { get; set; } = null!;

        [Column("seccion")]
        public string Seccion { get; set; } = null!;

        [Column("descripcion")]
        public string? Descripcion { get; set; }
    }
}
