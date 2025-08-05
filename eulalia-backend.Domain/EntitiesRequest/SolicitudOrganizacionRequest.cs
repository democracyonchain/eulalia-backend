using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eulalia_backend.Domain.EntitiesRequest
{
    public class SolicitudOrganizacionRequest
    {
        // Datos de la organización
        public string Nombre { get; set; } = default!;
        public string Tipo_Organizacion { get; set; } = default!;
        public int? Codigo_Provincia { get; set; }
        public int? Codigo_Canton { get; set; }
        public int? Codigo_Parroquia { get; set; }

        // Cédula del responsable (relacionado con ciudadano)
        public string Responsable_Cedula { get; set; } = default!;

        // Datos del ciudadano responsable (si es nuevo)
        public string Responsable_Nombre { get; set; } = default!;
        public string Responsable_Apellido { get; set; } = default!;
        public DateTime Responsable_FechaNacimiento { get; set; }
        public string Responsable_Direccion { get; set; } = default!;
        public string Responsable_Telefono { get; set; } = default!;
        public string Responsable_Email { get; set; } = default!;


        // Datos adicionales de la solicitud
        public string? Observaciones { get; set; }
    }
}
