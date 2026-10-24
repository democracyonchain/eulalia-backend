namespace eulalia_backend.Application.DTOs;

public sealed class IdentusConnectionStatus
{
    public string ConnectionId { get; init; } = string.Empty;
    public string? MyDid { get; init; }
    public string? TheirDid { get; init; }
    public string? State { get; init; }
}
