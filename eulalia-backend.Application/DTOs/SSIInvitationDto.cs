namespace eulalia_backend.Application.DTOs
{
    public class SSIInvitationDto
    {
        public string InvitationUrl { get; set; } = default!;
        public string InvitationId { get; set; } = default!;
        public string? QrCodeBase64 { get; set; }
    }
}
