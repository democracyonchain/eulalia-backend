namespace eulalia_backend.Application.Interfaces;

public sealed record CardanoSubmissionResult(
    string EventId,
    string Status,
    string? TxHash = null,
    string? ErrorCode = null);

public interface ICardanoClient
{
    Task<CardanoSubmissionResult> SubmitAffiliationAsync(
        int affiliationId,
        int organizationId,
        string recordHash,
        CancellationToken cancellationToken = default);

    Task<CardanoSubmissionResult> SubmitCancellationAsync(
        int affiliationId,
        int organizationId,
        string recordHash,
        CancellationToken cancellationToken = default);
}
