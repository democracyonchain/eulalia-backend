using eulalia_backend.Application.Interfaces;
using eulalia_backend.Application.Services;
using eulalia_backend.Domain.Entities;
using eulalia_backend.Infrastructure.Data;
using eulalia_backend.Infrastructure.Repositories;
using eulalia_backend.Infrastructure.Options;
using eulalia_backend.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;

namespace eulalia_backend.Tests.Services;

public sealed class AfiliacionCardanoTests
{
    [Fact]
    public async Task Create_PersistsCardanoResultWithoutSendingCedula()
    {
        await using var context = CreateContext();
        var client = new RecordingCardanoClient
        {
            AffiliationResult = new CardanoSubmissionResult("affiliation-1", "submitted", "tx-1")
        };
        var service = new AfiliacionService(new Repository<Afiliacion>(context), client);

        var result = await service.CreateAsync(new eulalia_backend.Application.DTOs.AfiliacionDto
        {
            Cedula = "1234567890",
            OrganizacionId = 7,
            FechaAfiliacion = DateTime.UtcNow
        });

        result.CardanoStatus.Should().Be("submitted");
        result.CardanoTxHash.Should().Be("tx-1");
        client.LastRecordHash.Should().HaveLength(64);
        client.LastRecordHash.Should().NotContain("1234567890");
        client.LastEventId.Should().Be("affiliation-1");
        var saved = await context.Afiliaciones.SingleAsync();
        saved.CardanoAffiliationTxHash.Should().Be("tx-1");
    }

    [Fact]
    public async Task Cancellation_PreservesAnuladoWhenCardanoFails()
    {
        await using var context = CreateContext();
        var affiliation = new Afiliacion { Cedula = "1234567890", OrganizacionId = 7 };
        context.Afiliaciones.Add(affiliation);
        await context.SaveChangesAsync();
        var client = new RecordingCardanoClient
        {
            CancellationResult = new CardanoSubmissionResult("cancellation-1", "failed", ErrorCode: "CARDANO_UNAVAILABLE")
        };
        var service = new AfiliacionService(new Repository<Afiliacion>(context), client);

        (await service.AnularAsync(affiliation.Afiliacion_Id)).Should().BeTrue();

        var saved = await context.Afiliaciones.FindAsync(affiliation.Afiliacion_Id);
        saved!.Estado.Should().Be("Anulado");
        saved.CardanoCancellationStatus.Should().Be("failed");
        saved.CardanoCancellationEventId.Should().Be("cancellation-1");
    }

    [Fact]
    public async Task CardanoClient_SendsBearerAndPrivacySafePayload()
    {
        string? body = null;
        var handler = new CaptureHandler(request =>
        {
            body = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
            request.Headers.Authorization!.Scheme.Should().Be("Bearer");
            request.Headers.Authorization.Parameter.Should().Be("test-token");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new { eventId = "affiliation-1", status = "submitted", txHash = "tx-1" })
            };
        });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://cardano.test/") };
        var client = new CardanoClient(httpClient, Options.Create(new CardanoOptions
        {
            Enabled = true,
            BaseUrl = "http://cardano.test",
            InternalApiToken = "test-token"
        }));

        var result = await client.SubmitAffiliationAsync(1, 7, new string('a', 64));

        result.Status.Should().Be("submitted");
        body.Should().NotContain("1234567890");
        body.Should().Contain("recordHash");
    }

    [Fact]
    public async Task CardanoClient_ReturnsPendingWhenDisabled()
    {
        using var httpClient = new HttpClient(new CaptureHandler(_ =>
            throw new InvalidOperationException("HTTP must not be called")));
        var client = new CardanoClient(httpClient, Options.Create(new CardanoOptions()));

        var result = await client.SubmitAffiliationAsync(1, 7, new string('a', 64));

        result.Status.Should().Be("pending");
        result.ErrorCode.Should().Be("CARDANO_NOT_CONFIGURED");
    }

    private static EulaliaContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<EulaliaContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new EulaliaContext(options);
    }

    private sealed class RecordingCardanoClient : ICardanoClient
    {
        public CardanoSubmissionResult AffiliationResult { get; init; } = new("affiliation-1", "pending");
        public CardanoSubmissionResult CancellationResult { get; init; } = new("cancellation-1", "pending");
        public string? LastRecordHash { get; private set; }
        public string? LastEventId { get; private set; }

        public Task<CardanoSubmissionResult> SubmitAffiliationAsync(int id, int organizationId, string recordHash, CancellationToken cancellationToken = default)
        {
            LastRecordHash = recordHash;
            LastEventId = AffiliationResult.EventId;
            return Task.FromResult(AffiliationResult);
        }

        public Task<CardanoSubmissionResult> SubmitCancellationAsync(int id, int organizationId, string recordHash, CancellationToken cancellationToken = default)
        {
            LastRecordHash = recordHash;
            LastEventId = CancellationResult.EventId;
            return Task.FromResult(CancellationResult);
        }
    }

    private sealed class CaptureHandler(Func<HttpRequestMessage, HttpResponseMessage> callback) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(callback(request));
    }
}
