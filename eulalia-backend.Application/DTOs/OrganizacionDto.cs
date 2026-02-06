namespace eulalia_backend.Application.DTOs
{
    public class OrganizacionDto
    {
        public int OrganizacionId { get; set; }
        public string Nombre { get; set; } = default!;
        public string Tipo { get; set; } = default!;
        public int? CodigoProvincia { get; set; }
        public int? CodigoCanton { get; set; }
        public int? CodigoParroquia { get; set; }
        public string ResponsableCedula { get; set; } = default!;
        public string Estado { get; set; } = "pendiente";
    }
}

