using eulalia_backend.Application.Interfaces;
using eulalia_backend.Application.Services;
using eulalia_backend.Domain.Entities;
using eulalia_backend.Domain.Enums;
using eulalia_backend.Infrastructure.Data;
using eulalia_backend.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace eulalia_backend.Tests;

public class SSIServiceTests
{
    private const string IssuingDid = "did:prism:configured-issuer";

    [Fact]
    public async Task CreateInvitation_ReturnsExistingActiveInvitation()
    {
        await using var context = CreateContext();
        var existing = AddInvitation(context);
        existing.InvitationId = "existing-id";
        existing.InvitationUrl = "existing-url";
        context.SaveChanges();
        var identus = new Mock<IIdentusClient>();
        var service = CreateService(context, identus);

        var result = await service.CreateInvitationAsync(existing.Cedula);

        result.InvitationId.Should().Be("existing-id");
        result.InvitationUrl.Should().Be("existing-url");
        identus.Verify(x => x.CreateInvitationAsync(It.IsAny<string>()), Times.Never);
        (await context.SsiIssuances.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task RetryInvitation_FailsOldIssuancesBeforeCreatingNewInvitation()
    {
        await using var context = CreateContext();
        var old = AddInvitation(context);
        old.InvitationId = "old-id";
        old.InvitationUrl = "old-url";
        context.SaveChanges();
        var identus = new Mock<IIdentusClient>();
        identus.Setup(x => x.CreateInvitationAsync("VoterID-12345678"))
            .ReturnsAsync(("new-url", "new-id"));
        var service = CreateService(context, identus);

        var result = await service.RetryInvitationAsync(old.Cedula);

        result.InvitationId.Should().Be("new-id");
        result.InvitationUrl.Should().Be("new-url");
        var issuances = await context.SsiIssuances.OrderBy(x => x.Id).ToListAsync();
        issuances.Should().HaveCount(2);
        issuances[0].Status.Should().Be(SsiIssuanceStatus.Failed);
        issuances[0].ErrorMessage.Should().Contain("retry");
        issuances[1].Status.Should().Be(SsiIssuanceStatus.InvitationGenerated);
        issuances.Count(x => x.Status == SsiIssuanceStatus.InvitationGenerated).Should().Be(1);
        identus.Verify(x => x.CreateInvitationAsync("VoterID-12345678"), Times.Once);
    }

    [Fact]
    public async Task HandleWebhookEvent_RealCloudAgentEnvelope_CreatesCredentialOffer()
    {
        const string connectionId = "connection-real";
        const string schemaId = "http://identus.test/schema-registry/schemas/citizen";
        await using var context = CreateContext();
        var issuance = AddInvitation(context);
        var identus = CreateIdentusMock(connectionId, schemaId);
        var service = CreateService(context, identus);

        await service.HandleWebhookEventAsync($"{{\"type\":\"ConnectionUpdated\",\"data\":{{\"connectionId\":\"{connectionId}\",\"label\":\"VoterID-{issuance.Cedula}\",\"state\":\"ConnectionResponseSent\"}}}}");

        var saved = await context.SsiIssuances.SingleAsync();
        saved.Status.Should().NotBe(SsiIssuanceStatus.InvitationGenerated);
        saved.CredentialRecordId.Should().Be("record-1");
        saved.HolderDid.Should().Be($"did:peer:1:{connectionId}");
        identus.Verify(x => x.CreateCredentialOfferAsync(IssuingDid, schemaId, connectionId, true), Times.Once);
    }

    [Fact]
    public async Task HandleWebhookEvent_ResponseReceivedState_DoesNotCreateCredentialOffer()
    {
        const string connectionId = "connection-inviter";
        const string schemaId = "http://identus.test/schema-registry/schemas/citizen";
        await using var context = CreateContext();
        var issuance = AddInvitation(context);
        var identus = CreateIdentusMock(connectionId, schemaId);
        var service = CreateService(context, identus);

        await service.HandleWebhookEventAsync($"{{\"type\":\"ConnectionUpdated\",\"data\":{{\"connectionId\":\"{connectionId}\",\"label\":\"VoterID-{issuance.Cedula}\",\"state\":\"ConnectionResponseReceived\"}}}}");

        (await context.SsiIssuances.SingleAsync()).Status.Should().Be(SsiIssuanceStatus.InvitationGenerated);
        identus.Verify(x => x.CreateCredentialOfferAsync(IssuingDid, schemaId, connectionId, true), Times.Never);
    }

    [Fact]
    public async Task HandleWebhookEvent_ResponseSentState_CreatesCredentialOffer()
    {
        const string connectionId = "connection-established";
        const string schemaId = "http://identus.test/schema-registry/schemas/citizen";
        await using var context = CreateContext();
        var issuance = AddInvitation(context);
        var identus = CreateIdentusMock(connectionId, schemaId);
        var service = CreateService(context, identus);

        await service.HandleWebhookEventAsync($"{{\"type\":\"ConnectionUpdated\",\"data\":{{\"connectionId\":\"{connectionId}\",\"label\":\"VoterID-{issuance.Cedula}\",\"state\":\"ConnectionResponseSent\"}}}}");

        (await context.SsiIssuances.SingleAsync()).Status.Should().Be(SsiIssuanceStatus.CredentialIssued);
        identus.Verify(x => x.CreateCredentialOfferAsync(IssuingDid, schemaId, connectionId, true), Times.Once);
    }

    [Fact]
    public async Task HandleWebhookEvent_PendingState_IsIgnoredUntilEligibleStateArrives()
    {
        const string connectionId = "connection-pending";
        const string schemaId = "http://identus.test/schema-registry/schemas/citizen";
        await using var context = CreateContext();
        var issuance = AddInvitation(context);
        var identus = CreateIdentusMock(connectionId, schemaId);
        var service = CreateService(context, identus);
        var label = $"VoterID-{issuance.Cedula}";

        await service.HandleWebhookEventAsync($"{{\"type\":\"ConnectionUpdated\",\"data\":{{\"connectionId\":\"{connectionId}\",\"label\":\"{label}\",\"state\":\"ConnectionResponsePending\"}}}}");

        (await context.SsiIssuances.SingleAsync()).Status.Should().Be(SsiIssuanceStatus.InvitationGenerated);
        identus.Verify(x => x.CreateCredentialOfferAsync(IssuingDid, schemaId, connectionId, true), Times.Never);

        await service.HandleWebhookEventAsync($"{{\"type\":\"ConnectionUpdated\",\"data\":{{\"connectionId\":\"{connectionId}\",\"label\":\"{label}\",\"state\":\"ConnectionResponseSent\"}}}}");

        (await context.SsiIssuances.SingleAsync()).Status.Should().Be(SsiIssuanceStatus.CredentialIssued);
        identus.Verify(x => x.CreateCredentialOfferAsync(IssuingDid, schemaId, connectionId, true), Times.Once);
    }

    [Fact]
    public async Task HandleWebhookEvent_RequestReceivedState_DoesNotCreateCredentialOffer()
    {
        const string connectionId = "connection-request-received";
        const string schemaId = "http://identus.test/schema-registry/schemas/citizen";
        await using var context = CreateContext();
        var issuance = AddInvitation(context);
        var identus = CreateIdentusMock(connectionId, schemaId);
        var service = CreateService(context, identus);

        await service.HandleWebhookEventAsync($"{{\"type\":\"ConnectionUpdated\",\"data\":{{\"connectionId\":\"{connectionId}\",\"label\":\"VoterID-{issuance.Cedula}\",\"state\":\"ConnectionRequestReceived\"}}}}");

        (await context.SsiIssuances.SingleAsync()).Status.Should().Be(SsiIssuanceStatus.InvitationGenerated);
        identus.Verify(x => x.CreateCredentialOfferAsync(IssuingDid, schemaId, connectionId, true), Times.Never);
    }

    [Fact]
    public async Task HandleWebhookEvent_RepeatedConnectionStates_CreatesOnlyOneCredentialOffer()
    {
        const string connectionId = "connection-duplicate";
        const string schemaId = "http://identus.test/schema-registry/schemas/citizen";
        await using var context = CreateContext();
        var issuance = AddInvitation(context);
        var identus = CreateIdentusMock(connectionId, schemaId);
        var service = CreateService(context, identus);
        var label = $"VoterID-{issuance.Cedula}";

        await service.HandleWebhookEventAsync($"{{\"type\":\"ConnectionUpdated\",\"data\":{{\"connectionId\":\"{connectionId}\",\"label\":\"{label}\",\"state\":\"ConnectionRequestReceived\"}}}}");
        await service.HandleWebhookEventAsync($"{{\"type\":\"ConnectionUpdated\",\"data\":{{\"connectionId\":\"{connectionId}\",\"label\":\"{label}\",\"state\":\"ConnectionResponseSent\"}}}}");

        identus.Verify(x => x.CreateCredentialOfferAsync(IssuingDid, schemaId, connectionId, true), Times.Once);
    }

    [Fact]
    public async Task HandleWebhookEvent_LegacyPayload_RemainsCompatible()
    {
        const string connectionId = "connection-legacy";
        const string schemaId = "http://identus.test/schema-registry/schemas/citizen";
        await using var context = CreateContext();
        var issuance = AddInvitation(context);
        var identus = CreateIdentusMock(connectionId, schemaId);
        var service = CreateService(context, identus);

        await service.HandleWebhookEventAsync($"{{\"eventType\":\"ConnectionEstablished\",\"connectionId\":\"{connectionId}\",\"theirLabel\":\"VoterID-{issuance.Cedula}\"}}");

        (await context.SsiIssuances.SingleAsync()).Status.Should().NotBe(SsiIssuanceStatus.InvitationGenerated);
        identus.Verify(x => x.CreateCredentialOfferAsync(IssuingDid, schemaId, connectionId, true), Times.Once);
    }

    [Fact]
    public async Task HandleWebhookEvent_MissingSchemaConfiguration_MarksIssuanceFailed()
    {
        const string connectionId = "connection-missing-schema";
        await using var context = CreateContext();
        var issuance = AddInvitation(context);
        var identus = CreateIdentusMock(connectionId, null);
        var service = CreateService(context, identus);

        await service.HandleWebhookEventAsync($"{{\"type\":\"ConnectionUpdated\",\"data\":{{\"connectionId\":\"{connectionId}\",\"label\":\"VoterID-{issuance.Cedula}\",\"state\":\"ConnectionResponseSent\"}}}}");

        var saved = await context.SsiIssuances.SingleAsync();
        saved.Status.Should().Be(SsiIssuanceStatus.Failed);
        saved.ErrorMessage.Should().Contain("schema configuration");
        identus.Verify(x => x.CreateCredentialOfferAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task HandleWebhookEvent_NullOfferResult_MarksIssuanceFailed()
    {
        const string connectionId = "connection-offer-failed";
        const string schemaId = "http://identus.test/schema-registry/schemas/citizen";
        await using var context = CreateContext();
        var issuance = AddInvitation(context);
        var identus = CreateIdentusMock(connectionId, schemaId);
        identus.Setup(x => x.CreateCredentialOfferAsync(IssuingDid, schemaId, connectionId, true))
            .ReturnsAsync(((string RecordId, string OfferUrl)?)null);
        var service = CreateService(context, identus);

        await service.HandleWebhookEventAsync($"{{\"type\":\"ConnectionUpdated\",\"data\":{{\"connectionId\":\"{connectionId}\",\"label\":\"VoterID-{issuance.Cedula}\",\"state\":\"ConnectionResponseSent\"}}}}");

        var saved = await context.SsiIssuances.SingleAsync();
        saved.Status.Should().Be(SsiIssuanceStatus.Failed);
        saved.ErrorMessage.Should().Contain("did not create");
    }

    private static SsiIssuance AddInvitation(EulaliaContext context)
    {
        var issuance = new SsiIssuance
        {
            Cedula = "12345678",
            Status = SsiIssuanceStatus.InvitationGenerated,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.SsiIssuances.Add(issuance);
        context.SaveChanges();
        return issuance;
    }

    private static Mock<IIdentusClient> CreateIdentusMock(string connectionId, string? schemaId)
    {
        var identus = new Mock<IIdentusClient>();
        identus.SetupGet(x => x.ConfiguredSchemaId).Returns(schemaId);
        identus.SetupGet(x => x.ConfiguredIssuingDid).Returns(IssuingDid);
        identus.Setup(x => x.CreateCredentialOfferAsync(IssuingDid, schemaId!, connectionId, true))
            .ReturnsAsync(("record-1", "offer-url"));
        return identus;
    }

    private static SSIService CreateService(EulaliaContext context, Mock<IIdentusClient> identus) =>
        new(new Repository<SsiIssuance>(context), identus.Object, NullLogger<SSIService>.Instance);

    private static EulaliaContext CreateContext() =>
        new(new DbContextOptionsBuilder<EulaliaContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
}
