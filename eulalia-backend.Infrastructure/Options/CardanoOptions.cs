namespace eulalia_backend.Infrastructure.Options;

public sealed class CardanoOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string InternalApiToken { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public int TimeoutSeconds { get; set; } = 10;
}
