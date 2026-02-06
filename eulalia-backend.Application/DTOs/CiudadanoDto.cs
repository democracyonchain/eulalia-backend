namespace eulalia_backend.Application.DTOs
{
    public class CiudadanoDto
    {
        public string Cedula { get; set; } = default!;
        public string Nombres { get; set; } = default!;
        public string Apellidos { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? Telefono { get; set; }
        public DateTime FechaNacimiento { get; set; }
    }
}
