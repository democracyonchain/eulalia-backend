using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using eulalia_backend.Application.DTOs;
using eulalia_backend.Application.Interfaces;
using eulalia_backend.Application.Services;
using eulalia_backend.Domain.Entities;
using eulalia_backend.Infrastructure.Data;
using eulalia_backend.Infrastructure.Options;
using eulalia_backend.Infrastructure.Repositories;
using eulalia_backend.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace eulalia_backend.Tests;

public class IdentityProvisioningTests
{
    [Fact]
    public async Task IdentusClient_ParsesDidAndPublicationReference()
    {
        var client = CreateClient((request, _) => Task.FromResult(Response(HttpStatusCode.OK,
            request.RequestUri!.AbsolutePath.EndsWith("publications")
                ? "{\"operationId\":\"op-123\",\"publishedPrismDID\":\"did:prism:short\"}"
                : "{\"longFormDid\":\"did:prism:long\"}")));

        var did = await client.CreateManagedDidAsync();
        var publication = await client.RequestDidPublicationAsync(did.LongFormDid);

        did.LongFormDid.Should().Be("did:prism:long");
        publication.OperationId.Should().Be("op-123");
        publication.PublishedDid.Should().Be("did:prism:short");
    }

    [Fact]
    public async Task IdentusClient_RejectsNonSuccessAndMalformedResponses()
    {
        var failed = CreateClient((_, _) => Task.FromResult(Response(HttpStatusCode.BadGateway, "gateway failure")));
        var act = () => failed.CreateManagedDidAsync();
        await act.Should().ThrowAsync<HttpRequestException>();

        var malformed = CreateClient((_, _) => Task.FromResult(Response(HttpStatusCode.OK, "not-json")));
        var malformedAct = () => malformed.CreateManagedDidAsync();
        await malformedAct.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task IdentusClient_ReadsConnectionStateAndDids()
    {
        var client = CreateClient((_, _) => Task.FromResult(Response(HttpStatusCode.OK,
            "{\"connectionId\":\"connection-1\",\"myDid\":\"did:peer:cloud\",\"theirDid\":\"did:key:wallet\",\"state\":\"ConnectionResponseReceived\"}")));

        var connection = await client.GetConnectionStatusAsync("connection-1");

        connection!.ConnectionId.Should().Be("connection-1");
        connection.MyDid.Should().Be("did:peer:cloud");
        connection.TheirDid.Should().Be("did:key:wallet");
        connection.State.Should().Be("ConnectionResponseReceived");
    }

    [Fact]
    public async Task IdentusClient_CredentialOfferTargetsHolderConnectionAndUsesConfiguredIssuerClaim()
    {
        string? requestBody = null;
        var client = CreateClient(async (request, _) =>
        {
            requestBody = await request.Content!.ReadAsStringAsync();
            return Response(HttpStatusCode.OK, "{\"recordId\":\"record-1\",\"offerUrl\":\"offer-1\"}");
        });

        var result = await client.CreateCredentialOfferAsync(
            "did:prism:configured-issuer",
            "http://identus.test/schema-registry/schemas/citizen",
            "connection-uuid");

        result!.Value.RecordId.Should().Be("record-1");
        using var body = JsonDocument.Parse(requestBody!);
        body.RootElement.GetProperty("connectionId").GetString().Should().Be("connection-uuid");
        body.RootElement.GetProperty("claims").GetProperty("organizationDid").GetString()
            .Should().Be("did:prism:configured-issuer");
    }

    [Fact]
    public async Task Provisioning_IsIdempotentAndReusesPersistedDid()
    {
        await using var context = CreateContext();
        context.Organizaciones.Add(new Organizacion { Organizacion_Id = 8, Nombre = "Demo", Tipo_Organizacion = "Partido", Responsable_Cedula = "1" });
        context.OrganizacionIdentidades.Add(new OrganizacionIdentidad { OrganizacionId = 8, ClientDid = "did:key:z6Mkwallet", Estado = "wallet_connected" });
        await context.SaveChangesAsync();
        var identus = new Mock<IIdentusClient>();
        identus.SetupGet(x => x.OrganizationCredentialConfigurationAvailable).Returns(true);
        identus.SetupGet(x => x.OrganizationGlobalHolderConnectionAvailable).Returns(true);
        identus.Setup(x => x.CreateOrganizationCredentialOfferAsync("did:key:z6Mkwallet", null, It.IsAny<CancellationToken>())).ReturnsAsync(("record-1", "offer-1"));
        var service = new OrganizacionIdentityService(new Repository<Organizacion>(context), new Repository<OrganizacionIdentidad>(context), identus.Object, NullLogger<OrganizacionIdentityService>.Instance);

        var first = await service.ProvisionAsync(8);
        var second = await service.ProvisionAsync(8);

        first.ClientDid.Should().Be(second.ClientDid);
        (await context.OrganizacionIdentidades.CountAsync()).Should().Be(1);
        identus.Verify(x => x.CreateOrganizationCredentialOfferAsync("did:key:z6Mkwallet", null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("ConnectionResponseSent")]
    [InlineData("ConnectionResponseReceived")]
    public async Task Provisioning_UsesPersistedEstablishedHolderConnection(string state)
    {
        await using var context = CreateContext();
        context.Organizaciones.Add(new Organizacion { Organizacion_Id = 8, Nombre = "Demo", Tipo_Organizacion = "Partido", Responsable_Cedula = "1" });
        context.OrganizacionIdentidades.Add(new OrganizacionIdentidad { OrganizacionId = 8, ClientDid = "did:key:z6Mkwallet", HolderConnectionId = "connection-org", Estado = "wallet_connected" });
        await context.SaveChangesAsync();
        var identus = new Mock<IIdentusClient>();
        identus.SetupGet(x => x.OrganizationCredentialConfigurationAvailable).Returns(true);
        identus.SetupGet(x => x.OrganizationGlobalHolderConnectionAvailable).Returns(true);
        identus.Setup(x => x.GetConnectionStatusAsync("connection-org", It.IsAny<CancellationToken>())).ReturnsAsync(new IdentusConnectionStatus
        {
            ConnectionId = "connection-org",
            MyDid = "did:peer:cloud-representation",
            TheirDid = "did:key:serialized-wallet-representation",
            State = state
        });
        identus.Setup(x => x.CreateOrganizationCredentialOfferAsync("did:key:z6Mkwallet", "connection-org", It.IsAny<CancellationToken>())).ReturnsAsync(("record-1", "offer-1"));
        var service = new OrganizacionIdentityService(new Repository<Organizacion>(context), new Repository<OrganizacionIdentidad>(context), identus.Object, NullLogger<OrganizacionIdentityService>.Instance);

        await service.ProvisionAsync(8);

        identus.Verify(x => x.CreateOrganizationCredentialOfferAsync("did:key:z6Mkwallet", "connection-org", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("ConnectionRequestReceived")]
    public async Task Provisioning_RejectsNonTerminalHolderConnection(string state)
    {
        await using var context = CreateContext();
        context.Organizaciones.Add(new Organizacion { Organizacion_Id = 8, Nombre = "Demo", Tipo_Organizacion = "Partido", Responsable_Cedula = "1" });
        context.OrganizacionIdentidades.Add(new OrganizacionIdentidad { OrganizacionId = 8, ClientDid = "did:key:z6Mkwallet", HolderConnectionId = "connection-org", Estado = "wallet_connected" });
        await context.SaveChangesAsync();
        var identus = new Mock<IIdentusClient>();
        identus.SetupGet(x => x.OrganizationCredentialConfigurationAvailable).Returns(true);
        identus.SetupGet(x => x.OrganizationGlobalHolderConnectionAvailable).Returns(true);
        identus.Setup(x => x.GetConnectionStatusAsync("connection-org", It.IsAny<CancellationToken>())).ReturnsAsync(new IdentusConnectionStatus
        {
            ConnectionId = "connection-org",
            TheirDid = "did:key:serialized-wallet-representation",
            State = state
        });
        var service = CreateIdentityService(context, identus);

        var result = await service.ProvisionAsync(8);

        result.Estado.Should().Be("credential_pending");
        result.ErrorMessage.Should().Contain("no está establecida");
        identus.Verify(x => x.CreateOrganizationCredentialOfferAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RetryCredential_ReplacesRecordAndPreservesWalletDidAndConnection()
    {
        await using var context = CreateContext();
        context.Organizaciones.Add(new Organizacion { Organizacion_Id = 8, Nombre = "Demo", Tipo_Organizacion = "Partido", Responsable_Cedula = "1" });
        context.OrganizacionIdentidades.Add(new OrganizacionIdentidad
        {
            OrganizacionId = 8,
            ClientDid = "did:key:z6Mkwallet",
            HolderConnectionId = "connection-org",
            CredentialRecordId = "old-record",
            CredentialStatus = "credential_issued_pending_holder_receipt",
            Estado = "credential_pending",
            UpdatedAt = DateTime.UtcNow.AddMinutes(-5)
        });
        await context.SaveChangesAsync();
        var identus = new Mock<IIdentusClient>();
        identus.SetupGet(x => x.OrganizationCredentialConfigurationAvailable).Returns(true);
        identus.Setup(x => x.GetConnectionStatusAsync("connection-org", It.IsAny<CancellationToken>())).ReturnsAsync(new IdentusConnectionStatus
        {
            ConnectionId = "connection-org",
            TheirDid = "did:key:z6Mkwallet",
            State = "ConnectionResponseReceived"
        });
        identus.Setup(x => x.CreateOrganizationCredentialOfferAsync("did:key:z6Mkwallet", "connection-org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(("new-record", "offer-url"));
        var service = CreateIdentityService(context, identus);

        var result = await service.RetryOrganizationCredentialAsync(8);

        result.CredentialRecordId.Should().Be("new-record");
        result.ClientDid.Should().Be("did:key:z6Mkwallet");
        result.HolderConnectionId.Should().Be("connection-org");
        result.CredentialStatus.Should().Be("offer_pending_holder_acceptance");
        result.Estado.Should().Be("credential_pending");
        identus.Verify(x => x.CreateOrganizationCredentialOfferAsync("did:key:z6Mkwallet", "connection-org", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RetryCredential_RejectsImmediateRepeatAndDoesNotCallIdentus()
    {
        await using var context = CreateContext();
        context.Organizaciones.Add(new Organizacion { Organizacion_Id = 8, Nombre = "Demo", Tipo_Organizacion = "Partido", Responsable_Cedula = "1" });
        context.OrganizacionIdentidades.Add(new OrganizacionIdentidad
        {
            OrganizacionId = 8,
            ClientDid = "did:key:z6Mkwallet",
            HolderConnectionId = "connection-org",
            CredentialRecordId = "record-1",
            CredentialStatus = "credential_issued_pending_holder_receipt",
            Estado = "credential_pending",
            UpdatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
        var identus = new Mock<IIdentusClient>();
        identus.SetupGet(x => x.OrganizationCredentialConfigurationAvailable).Returns(true);
        var service = CreateIdentityService(context, identus);

        var act = () => service.RetryOrganizationCredentialAsync(8);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*conexión*reofertar*");
        identus.Verify(x => x.CreateOrganizationCredentialOfferAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RetryCredential_OfferPendingWithEstablishedConnectionCreatesReplacementOffer()
    {
        await using var context = CreateContext();
        context.Organizaciones.Add(new Organizacion { Organizacion_Id = 8, Nombre = "Demo", Tipo_Organizacion = "Partido", Responsable_Cedula = "1" });
        context.OrganizacionIdentidades.Add(new OrganizacionIdentidad
        {
            OrganizacionId = 8,
            ClientDid = "did:key:z6Mkwallet",
            HolderConnectionId = "connection-org",
            CredentialRecordId = "old-record",
            CredentialStatus = "offer_pending_holder_acceptance",
            Estado = "credential_pending",
            UpdatedAt = DateTime.UtcNow.AddMinutes(-5)
        });
        await context.SaveChangesAsync();
        var identus = new Mock<IIdentusClient>();
        identus.SetupGet(x => x.OrganizationCredentialConfigurationAvailable).Returns(true);
        identus.Setup(x => x.GetConnectionStatusAsync("connection-org", It.IsAny<CancellationToken>())).ReturnsAsync(new IdentusConnectionStatus
        {
            ConnectionId = "connection-org", MyDid = "did:peer:cloud-representation", TheirDid = "did:key:serialized-wallet-representation", State = "ConnectionResponseReceived"
        });
        identus.Setup(x => x.CreateOrganizationCredentialOfferAsync("did:key:z6Mkwallet", "connection-org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(("new-record", "new-offer-url"));
        var service = CreateIdentityService(context, identus);

        var result = await service.RetryOrganizationCredentialAsync(8);

        result.CredentialRecordId.Should().Be("new-record");
        result.CredentialStatus.Should().Be("offer_pending_holder_acceptance");
        result.Estado.Should().Be("credential_pending");
        identus.Verify(x => x.CreateOrganizationCredentialOfferAsync("did:key:z6Mkwallet", "connection-org", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RetryCredential_OfferPendingWithStaleConnectionRequiresRecoveryBeforeOffer()
    {
        await using var context = CreateContext();
        context.Organizaciones.Add(new Organizacion { Organizacion_Id = 8, Nombre = "Demo", Tipo_Organizacion = "Partido", Responsable_Cedula = "1" });
        context.OrganizacionIdentidades.Add(new OrganizacionIdentidad
        {
            OrganizacionId = 8, ClientDid = "did:key:z6Mkwallet", HolderConnectionId = "stale",
            CredentialRecordId = "old-record", CredentialStatus = "offer_pending_holder_acceptance", Estado = "credential_pending",
            UpdatedAt = DateTime.UtcNow.AddMinutes(-5)
        });
        await context.SaveChangesAsync();
        var identus = new Mock<IIdentusClient>();
        identus.SetupGet(x => x.OrganizationCredentialConfigurationAvailable).Returns(true);
        identus.Setup(x => x.GetConnectionStatusAsync("stale", It.IsAny<CancellationToken>())).ReturnsAsync((IdentusConnectionStatus?)null);
        var service = CreateIdentityService(context, identus);

        var act = () => service.RetryOrganizationCredentialAsync(8);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*recuperarse*validarse*");
        identus.Verify(x => x.CreateOrganizationCredentialOfferAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProvisioningFailure_DoesNotApproveRequest()
    {
        await using var context = CreateContext();
        context.Organizaciones.Add(new Organizacion { Organizacion_Id = 8, Nombre = "Demo", Tipo_Organizacion = "Partido", Responsable_Cedula = "1" });
        context.OrganizacionIdentidades.Add(new OrganizacionIdentidad { OrganizacionId = 8, ClientDid = "did:key:z6Mkwallet", Estado = "wallet_connected" });
        var request = new SolicitudOrganizacion { Solicitud_Id = 4, Organizacion_Id = 8, Organizacion = context.Organizaciones.Local.Single(), Estado = "pendiente" };
        context.SolicitudOrganizacion.Add(request);
        await context.SaveChangesAsync();
        var identus = new Mock<IIdentusClient>();
        identus.SetupGet(x => x.OrganizationCredentialConfigurationAvailable).Returns(true);
        identus.SetupGet(x => x.OrganizationGlobalHolderConnectionAvailable).Returns(true);
        identus.Setup(x => x.CreateOrganizationCredentialOfferAsync("did:key:z6Mkwallet", null, It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException("Identus unavailable"));
        var service = new OrganizacionIdentityService(new Repository<Organizacion>(context), new Repository<OrganizacionIdentidad>(context), identus.Object, NullLogger<OrganizacionIdentityService>.Instance);
        var controller = new eulalia_backend.Api.Controllers.SolicitudOrganizacionController(context, service)
        {
            ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim("rol", "1") }))
                }
            }
        };

        var result = await controller.ActualizarEstado(4, new eulalia_backend.Domain.EntitiesRequest.UpdateEstadoSolicitudOrganizacionRequest { Estado = "aprobado" });

        result.Should().BeOfType<Microsoft.AspNetCore.Mvc.ObjectResult>().Which.StatusCode.Should().Be(409);
        (await context.SolicitudOrganizacion.FindAsync(4))!.Estado.Should().Be("pendiente");
        (await context.OrganizacionIdentidades.SingleAsync()).Estado.Should().Be("failed");
    }

    [Fact]
    public async Task CredentialPending_ReturnsExplicitErrorCodeAndDoesNotApproveRequest()
    {
        await using var context = CreateContext();
        context.Organizaciones.Add(new Organizacion { Organizacion_Id = 8, Nombre = "Demo", Tipo_Organizacion = "Partido", Responsable_Cedula = "1" });
        context.OrganizacionIdentidades.Add(new OrganizacionIdentidad { OrganizacionId = 8, ClientDid = "did:key:z6Mkwallet", Estado = "wallet_connected" });
        var request = new SolicitudOrganizacion { Solicitud_Id = 4, Organizacion_Id = 8, Organizacion = context.Organizaciones.Local.Single(), Estado = "pendiente" };
        context.SolicitudOrganizacion.Add(request);
        await context.SaveChangesAsync();
        var identus = new Mock<IIdentusClient>();
        identus.SetupGet(x => x.OrganizationCredentialConfigurationAvailable).Returns(false);
        var service = new OrganizacionIdentityService(new Repository<Organizacion>(context), new Repository<OrganizacionIdentidad>(context), identus.Object, NullLogger<OrganizacionIdentityService>.Instance);
        var controller = new eulalia_backend.Api.Controllers.SolicitudOrganizacionController(context, service)
        {
            ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim("rol", "1") }))
                }
            }
        };

        var result = await controller.ActualizarEstado(4, new eulalia_backend.Domain.EntitiesRequest.UpdateEstadoSolicitudOrganizacionRequest { Estado = "aprobado" });

        var conflict = result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflict.StatusCode.Should().Be(409);
        var response = conflict.Value!;
        response.GetType().GetProperty("error")!.GetValue(response).Should().Be("credential_pending");
        response.GetType().GetProperty("message")!.GetValue(response).Should().Be("La credencial de autorización todavía no está emitida; la solicitud permanece pendiente.");
        (await context.SolicitudOrganizacion.FindAsync(4))!.Estado.Should().Be("pendiente");
        (await context.OrganizacionIdentidades.SingleAsync()).Estado.Should().Be("credential_pending");
    }

    [Theory]
    [InlineData("accepted")]
    [InlineData("credential-issued")]
    [InlineData("credential-received")]
    [InlineData("done")]
    public async Task RefreshCredentialStatus_CompletesForKnownAcceptedStates(string protocolState)
    {
        await using var context = CreateContextWithIdentity();
        var identus = CreateIdentusMock(protocolState);
        var service = CreateIdentityService(context, identus);

        var identity = await service.RefreshCredentialStatusAsync(8);

        identity!.Estado.Should().Be("completed");
        identity.CredentialStatus.Should().Be("issued");
    }

    [Fact]
    public async Task RefreshCredentialStatus_PreservesStateWhenIdentusFails()
    {
        await using var context = CreateContextWithIdentity();
        var identus = new Mock<IIdentusClient>();
        identus.Setup(x => x.GetCredentialRecordStatusAsync("record-1"))
            .ThrowsAsync(new HttpRequestException("Identus unavailable"));
        var service = CreateIdentityService(context, identus);

        var identity = await service.RefreshCredentialStatusAsync(8);

        identity!.Estado.Should().Be("credential_pending");
        identity.CredentialStatus.Should().Be("offer_pending_holder_acceptance");
        identity.ErrorMessage.Should().Contain("temporalmente");
    }

    [Fact]
    public async Task RefreshCredentialStatus_IgnoresUnknownProtocolState()
    {
        await using var context = CreateContextWithIdentity();
        var identus = CreateIdentusMock("request-received");
        var service = CreateIdentityService(context, identus);

        var identity = await service.RefreshCredentialStatusAsync(8);

        identity!.Estado.Should().Be("credential_pending");
        identity.CredentialStatus.Should().Be("offer_pending_holder_acceptance");
    }

    private static IdentusClient CreateClient(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
    {
        var httpClient = new HttpClient(new DelegateHandler(handler));
        return new IdentusClient(httpClient, Options.Create(new IdentusOptions
        {
            BaseUrl = "http://identus.test",
            TimeoutSeconds = 5,
            OrganizationIssuingDid = "did:prism:configured-issuer"
        }), NullLogger<IdentusClient>.Instance);
    }

    private static HttpResponseMessage Response(HttpStatusCode status, string body) => new(status) { Content = new StringContent(body) };

    private static EulaliaContext CreateContext() => new(new DbContextOptionsBuilder<EulaliaContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static EulaliaContext CreateContextWithIdentity()
    {
        var context = CreateContext();
        context.Organizaciones.Add(new Organizacion { Organizacion_Id = 8, Nombre = "Demo", Tipo_Organizacion = "Partido", Responsable_Cedula = "1" });
        context.OrganizacionIdentidades.Add(new OrganizacionIdentidad
        {
            OrganizacionId = 8,
            CredentialRecordId = "record-1",
            Estado = "credential_pending",
            CredentialStatus = "offer_pending_holder_acceptance"
        });
        context.SaveChanges();
        return context;
    }

    private static Mock<IIdentusClient> CreateIdentusMock(string protocolState)
    {
        var identus = new Mock<IIdentusClient>();
        identus.Setup(x => x.GetCredentialRecordStatusAsync("record-1")).ReturnsAsync(protocolState);
        return identus;
    }

    private static OrganizacionIdentityService CreateIdentityService(EulaliaContext context, Mock<IIdentusClient> identus) =>
        new(new Repository<Organizacion>(context), new Repository<OrganizacionIdentidad>(context), identus.Object, NullLogger<OrganizacionIdentityService>.Instance);

    private sealed class DelegateHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => handler(request, cancellationToken);
    }
}
