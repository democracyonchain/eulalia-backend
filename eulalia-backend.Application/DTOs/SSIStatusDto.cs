namespace eulalia_backend.Application.DTOs
{
    public class SSIStatusDto
    {
        public string Status { get; set; } = default!;
        public DateTime LastUpdated { get; set; }
        public string? Details { get; set; }
        public string? Error { get; set; }
    }
}
