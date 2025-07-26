using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eulalia_backend.Domain.Entities
{
    [Table("blockchain")]
    public class Blockchain
    {
        public int BlockchainId { get; set; }  
        public string HashTransaccion { get; set; } = default!;
        public string TipoTransaccion { get; set; } = default!;
        public string Data { get; set; } = default!;
        public DateTime FechaTransaccion { get; set; } = DateTime.UtcNow;
    }
}
