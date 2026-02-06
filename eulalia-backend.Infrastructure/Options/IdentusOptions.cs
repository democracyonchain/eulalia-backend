namespace eulalia_backend.Infrastructure.Options
{
    public class IdentusOptions
    {
        public const string Identus = "Identus";

        public string BaseUrl { get; set; } = "http://localhost:8080/cloud-agent";
        public string? ApiKey { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
    }
}
