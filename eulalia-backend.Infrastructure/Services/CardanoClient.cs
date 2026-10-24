using System.Net.Http.Headers;
using System.Net.Http.Json;
using eulalia_backend.Application.Interfaces;
using eulalia_backend.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace eulalia_backend.Infrastructure.Services;

public sealed class CardanoClient : ICardanoClient
{
    private readonly HttpClient _httpClient;
    private readonly CardanoOptions _options;

    public CardanoClient(HttpClient httpClient, IOptions<CardanoOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public Task<CardanoSubmissionResult> SubmitAffiliationAsync(
        int affiliationId, int organizationId, string recordHash, CancellationToken cancellationToken = default) =>
        SubmitAsync("affiliation", $"affiliation-{affiliationId}", affiliationId, organizationId, recordHash, cancellationToken);

    public Task<CardanoSubmissionResult> SubmitCancellationAsync(
        int affiliationId, int organizationId, string recordHash, CancellationToken cancellationToken = default) =>
        SubmitAsync("cancellation", $"cancellation-{affiliationId}", affiliationId, organizationId, recordHash, cancellationToken);

    private async Task<CardanoSubmissionResult> SubmitAsync(
        string eventType,
        string eventId,
        int affiliationId,
        int organizationId,
        string recordHash,
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.BaseUrl) || string.IsNullOrWhiteSpace(_options.InternalApiToken))
            return new CardanoSubmissionResult(eventId, "pending", ErrorCode: "CARDANO_NOT_CONFIGURED");

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"api/transactions/{(eventType == "affiliation" ? "affiliations" : "cancellations")}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.InternalApiToken);
            request.Content = JsonContent.Create(new
            {
                eventId,
                affiliationId = $"aff-{affiliationId}",
                organizationId = $"org-{organizationId}",
                recordHash
            });

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return new CardanoSubmissionResult(eventId, "failed", ErrorCode: "CARDANO_HTTP_FAILURE");

            var result = await response.Content.ReadFromJsonAsync<CardanoResponse>(cancellationToken: cancellationToken);
            return new CardanoSubmissionResult(
                result?.EventId ?? eventId,
                result?.Status ?? "pending",
                result?.TxHash,
                result?.ErrorCode);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new CardanoSubmissionResult(eventId, "failed", ErrorCode: "CARDANO_TIMEOUT");
        }
        catch (HttpRequestException)
        {
            return new CardanoSubmissionResult(eventId, "failed", ErrorCode: "CARDANO_UNAVAILABLE");
        }
    }

    private sealed record CardanoResponse(string? EventId, string? Status, string? TxHash, string? ErrorCode);
}
