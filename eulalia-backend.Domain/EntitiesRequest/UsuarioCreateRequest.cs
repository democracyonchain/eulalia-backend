using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eulalia_backend.Domain.EntitiesRequest
{
    public class UsuarioCreateRequest
    {
        public string Cedula_Ciudadano { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public DateTime Fecha_Nacimiento { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }

        public string Correo { get; set; }
        public string Contrasena { get; set; }
        public int Rol_Id { get; set; } 
    }
}
