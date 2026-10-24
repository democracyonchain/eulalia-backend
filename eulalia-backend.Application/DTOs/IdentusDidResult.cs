namespace eulalia_backend.Application.DTOs;

public record IdentusDidResult(string LongFormDid);
public record IdentusPublicationResult(string OperationId, string? PublishedDid);
