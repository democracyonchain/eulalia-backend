namespace eulalia_backend.Application.DTOs
{
    public class AfiliacionDto
    {
        public int AfiliacionId { get; set; }
        public string Cedula { get; set; } = default!;
        public int OrganizacionId { get; set; }
        public DateTime FechaAfiliacion { get; set; }
        public string Estado { get; set; } = default!;
    }
}
