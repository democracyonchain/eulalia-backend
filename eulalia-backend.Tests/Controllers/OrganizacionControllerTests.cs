using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using eulalia_backend.Api.Controllers;
using eulalia_backend.Application.DTOs;
using eulalia_backend.Application.Interfaces;
using eulalia_backend.Application.Services;
using eulalia_backend.Domain.Entities;
using eulalia_backend.Infrastructure.Data;
using eulalia_backend.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace eulalia_backend.Tests.Controllers
{
    public class OrganizacionControllerTests
    {
        private (EulaliaContext, OrganizacionController) GetController()
        {
            var options = new DbContextOptionsBuilder<EulaliaContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new EulaliaContext(options);
            var repository = new Repository<Organizacion>(context);
            var service = new OrganizacionService(repository);
            var controller = new OrganizacionController(service, new Repository<OrganizacionIdentidad>(context));
            return (context, controller);
        }

        [Fact]
        public async Task Create_Should_AddOrganizacion_With_NewFields()
        {
            // Arrange
            var (context, controller) = GetController();
            var dto = new OrganizacionDto
            {
                Nombre = "Org Test",
                Tipo = "Tipo Test",
                CodigoProvincia = 1,
                CodigoCanton = 2,
                CodigoParroquia = 3,
                ResponsableCedula = "1234567890",
                Estado = "pendiente"
            };

            // Act
            var result = await controller.Create(dto);

            // Assert
            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            var createdDto = createdResult.Value.Should().BeOfType<OrganizacionDto>().Subject;

            createdDto.OrganizacionId.Should().BeGreaterThan(0);
            createdDto.CodigoCanton.Should().Be(2);
            createdDto.CodigoParroquia.Should().Be(3);
            createdDto.ResponsableCedula.Should().Be("1234567890");

            var entity = await context.Organizaciones.FindAsync(createdDto.OrganizacionId);
            entity.Should().NotBeNull();
            entity!.Codigo_Canton.Should().Be(2);
            entity.Codigo_Parroquia.Should().Be(3);
            entity.Responsable_Cedula.Should().Be("1234567890");
        }

        [Fact]
        public async Task Update_Should_ModifyOrganizacion_With_NewFields()
        {
            // Arrange
            var (context, controller) = GetController();
            var dto = new OrganizacionDto
            {
                Nombre = "Org Inicial",
                Tipo = "Tipo",
                ResponsableCedula = "111"
            };
            var createResult = await controller.Create(dto);
            var createdId = ((OrganizacionDto)((CreatedAtActionResult)createResult.Result!).Value!).OrganizacionId;

            var updateDto = new OrganizacionDto
            {
                Nombre = "Org Mod",
                Tipo = "Tipo Mod",
                CodigoProvincia = 10,
                CodigoCanton = 20,
                CodigoParroquia = 30,
                ResponsableCedula = "222",
                Estado = "aprobado"
            };

            // Act
            var result = await controller.Update(createdId, updateDto);

            // Assert
            result.Should().BeOfType<NoContentResult>();

            var entity = await context.Organizaciones.FindAsync(createdId);
            entity.Should().NotBeNull();
            entity!.Nombre.Should().Be("Org Mod");
            entity.Codigo_Canton.Should().Be(20);
            entity.Codigo_Parroquia.Should().Be(30);
            entity.Responsable_Cedula.Should().Be("222");
        }

        [Fact]
        public async Task GetIdentidad_Should_ReturnOnlyClaimedOrganizationData()
        {
            var (context, controller) = GetController();
            var createResult = await controller.Create(new OrganizacionDto
            {
                Nombre = "Org Identidad",
                Tipo = "Tipo",
                ResponsableCedula = "123"
            });
            var createdId = ((OrganizacionDto)((CreatedAtActionResult)createResult.Result!).Value!).OrganizacionId;
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim("organizacion_id", createdId.ToString())
                    }))
                }
            };

            var result = await controller.GetIdentidad();

            var identity = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dto = identity.Value.Should().BeOfType<OrganizacionIdentidadDto>().Subject;
            dto.OrganizacionId.Should().Be(createdId);
            dto.Nombre.Should().Be("Org Identidad");
            dto.Did.Should().BeNull();
            dto.ReferenciaOperacion.Should().BeNull();
        }

        [Fact]
        public async Task AsociarDid_ShouldPersistOnlyPublicDidForClaimedOrganization()
        {
            var (context, controller) = GetController();
            var createResult = await controller.Create(new OrganizacionDto { Nombre = "Org Wallet", Tipo = "Tipo", ResponsableCedula = "123" });
            var organizationId = ((OrganizacionDto)((CreatedAtActionResult)createResult.Result!).Value!).OrganizacionId;
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim("organizacion_id", organizationId.ToString()),
                        new Claim("rol", "2")
                    }))
                }
            };

            var result = await controller.AsociarDid(new ClientDidRequest { Did = "did:key:z6Mkpubr" });

            result.Should().BeOfType<OkObjectResult>();
            var identity = await context.OrganizacionIdentidades.SingleAsync();
            identity.ClientDid.Should().Be("did:key:z6Mkpubr");
            identity.LongFormDid.Should().BeNull();
            identity.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public async Task AsociarDid_ShouldRejectInvalidDid()
        {
            var (context, controller) = GetController();
            var createResult = await controller.Create(new OrganizacionDto { Nombre = "Org Invalid", Tipo = "Tipo", ResponsableCedula = "456" });
            var organizationId = ((OrganizacionDto)((CreatedAtActionResult)createResult.Result!).Value!).OrganizacionId;
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim("organizacion_id", organizationId.ToString()),
                        new Claim("rol", "2")
                    }))
                }
            };

            var result = await controller.AsociarDid(new ClientDidRequest { Did = "private-key" });

            result.Should().BeOfType<BadRequestObjectResult>();
            (await context.OrganizacionIdentidades.CountAsync()).Should().Be(0);
        }

        [Theory]
        [InlineData("undefined")]
        [InlineData("did:key:z6Mkpub l")]
        [InlineData("did:key:z0Mkpubr")]
        public async Task AsociarDid_ShouldRejectInvalidWalletDid(string did)
        {
            var (_, controller) = GetController();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim("organizacion_id", "8"),
                        new Claim("rol", "2")
                    }))
                }
            };

            var result = await controller.AsociarDid(new ClientDidRequest { Did = did });

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AsociarDid_ShouldAllowReplacingDidWhilePendingWithoutCredential()
        {
            var (context, controller) = GetController();
            context.Organizaciones.Add(new Organizacion
            {
                Organizacion_Id = 8,
                Nombre = "Org Wallet",
                Tipo_Organizacion = "Tipo",
                Responsable_Cedula = "123",
                Estado_Validacion = "pendiente"
            });
            context.OrganizacionIdentidades.Add(new OrganizacionIdentidad
            {
                OrganizacionId = 8,
                ClientDid = "did:key:z6Mkoldr",
                Estado = "credential_pending"
            });
            await context.SaveChangesAsync();
            SetWalletClaims(controller, 8);

            var result = await controller.AsociarDid(new ClientDidRequest { Did = "did:key:z6Mknewr" });

            result.Should().BeOfType<OkObjectResult>();
            (await context.OrganizacionIdentidades.SingleAsync()).ClientDid.Should().Be("did:key:z6Mknewr");
        }

        [Theory]
        [InlineData("aprobado", null)]
        [InlineData("pendiente", "record-1")]
        [InlineData("pendiente", "")]
        public async Task AsociarDid_ShouldRejectReplacingDidAfterFinalizationOrCredential(string estado, string? recordId)
        {
            var (context, controller) = GetController();
            context.Organizaciones.Add(new Organizacion
            {
                Organizacion_Id = 8,
                Nombre = "Org Wallet",
                Tipo_Organizacion = "Tipo",
                Responsable_Cedula = "123",
                Estado_Validacion = estado
            });
            context.OrganizacionIdentidades.Add(new OrganizacionIdentidad
            {
                OrganizacionId = 8,
                ClientDid = "did:key:z6Mkoldr",
                CredentialRecordId = recordId,
                CredentialStatus = recordId == "" ? "accepted" : null
            });
            await context.SaveChangesAsync();
            SetWalletClaims(controller, 8);

            var result = await controller.AsociarDid(new ClientDidRequest { Did = "did:key:z6Mknewr" });

            result.Should().BeOfType<ConflictObjectResult>();
            (await context.OrganizacionIdentidades.SingleAsync()).ClientDid.Should().Be("did:key:z6Mkoldr");
        }

        [Fact]
        public async Task AsociarConexion_ShouldPersistAndAllowReplacementBeforeIssuance()
        {
            var (context, controller) = GetController();
            context.Organizaciones.Add(new Organizacion { Organizacion_Id = 8, Nombre = "Org", Tipo_Organizacion = "Tipo", Responsable_Cedula = "123", Estado_Validacion = "pendiente" });
            await context.SaveChangesAsync();
            SetWalletClaims(controller, 8);

            (await controller.AsociarConexion(new HolderConnectionRequest { HolderConnectionId = "connection-1" })).Should().BeOfType<OkObjectResult>();
            (await controller.AsociarConexion(new HolderConnectionRequest { HolderConnectionId = "connection-2" })).Should().BeOfType<OkObjectResult>();

            (await context.OrganizacionIdentidades.SingleAsync()).HolderConnectionId.Should().Be("connection-2");
        }

        [Theory]
        [InlineData("offer_pending_holder_acceptance")]
        [InlineData("credential_pending")]
        public async Task AsociarConexion_ShouldAllowReplacingConnectionWithPendingCredential(string credentialStatus)
        {
            var (context, controller) = GetController();
            context.Organizaciones.Add(new Organizacion { Organizacion_Id = 8, Nombre = "Org", Tipo_Organizacion = "Tipo", Responsable_Cedula = "123", Estado_Validacion = "pendiente" });
            context.OrganizacionIdentidades.Add(new OrganizacionIdentidad
            {
                OrganizacionId = 8,
                HolderConnectionId = "connection-old",
                CredentialRecordId = "record-1",
                CredentialStatus = credentialStatus,
                Estado = "credential_pending"
            });
            await context.SaveChangesAsync();
            SetWalletClaims(controller, 8);

            var result = await controller.AsociarConexion(new HolderConnectionRequest { HolderConnectionId = "connection-new" });

            result.Should().BeOfType<OkObjectResult>();
            var identity = await context.OrganizacionIdentidades.SingleAsync();
            identity.HolderConnectionId.Should().Be("connection-new");
            identity.CredentialRecordId.Should().Be("record-1");
        }

        [Fact]
        public async Task RecuperarConexionHolder_ShouldReplaceStaleConnectionAndPreserveWalletDid()
        {
            var (context, controller) = GetControllerWithIdentus(out var identus);
            context.Organizaciones.Add(new Organizacion { Organizacion_Id = 8, Nombre = "Org", Tipo_Organizacion = "Tipo", Responsable_Cedula = "123", Estado_Validacion = "pendiente" });
            context.OrganizacionIdentidades.Add(new OrganizacionIdentidad
            {
                OrganizacionId = 8,
                ClientDid = "did:key:wallet",
                HolderConnectionId = "stale-connection",
                Estado = "credential_pending",
                UpdatedAt = DateTime.UtcNow.AddMinutes(-2)
            });
            await context.SaveChangesAsync();
            SetWalletClaims(controller, 8);
            identus.Setup(x => x.GetConnectionStatusAsync("stale-connection", It.IsAny<CancellationToken>())).ReturnsAsync((IdentusConnectionStatus?)null);
            identus.Setup(x => x.CreateInvitationAsync(It.IsAny<string>())).ReturnsAsync(("https://identus.test/oob", "new-connection"));

            var result = await controller.RecuperarConexionHolder();

            result.Should().BeOfType<OkObjectResult>();
            var identity = await context.OrganizacionIdentidades.SingleAsync();
            identity.ClientDid.Should().Be("did:key:wallet");
            identity.HolderConnectionId.Should().Be("new-connection");
            identus.Verify(x => x.CreateInvitationAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RecuperarConexionHolder_ShouldNotReplaceEstablishedConnection()
        {
            var (context, controller) = GetControllerWithIdentus(out var identus);
            context.Organizaciones.Add(new Organizacion { Organizacion_Id = 8, Nombre = "Org", Tipo_Organizacion = "Tipo", Responsable_Cedula = "123", Estado_Validacion = "pendiente" });
            context.OrganizacionIdentidades.Add(new OrganizacionIdentidad
            {
                OrganizacionId = 8,
                ClientDid = "did:key:wallet",
                HolderConnectionId = "established",
                Estado = "credential_pending",
                UpdatedAt = DateTime.UtcNow.AddMinutes(-2)
            });
            await context.SaveChangesAsync();
            SetWalletClaims(controller, 8);
            identus.Setup(x => x.GetConnectionStatusAsync("established", It.IsAny<CancellationToken>())).ReturnsAsync(new IdentusConnectionStatus
            {
                ConnectionId = "established",
                MyDid = "did:peer:cloud-representation",
                TheirDid = "did:key:serialized-wallet-representation",
                State = "ConnectionResponseReceived"
            });

            var result = await controller.RecuperarConexionHolder();

            result.Should().BeOfType<OkObjectResult>();
            (await context.OrganizacionIdentidades.SingleAsync()).HolderConnectionId.Should().Be("established");
            identus.Verify(x => x.CreateInvitationAsync(It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData("ConnectionResponseSent")]
        [InlineData("ConnectionResponseReceived")]
        public async Task EstadoConexionHolder_ShouldReportBothEstablishedConnectionStates(string state)
        {
            var (context, controller) = GetControllerWithIdentus(out var identus);
            context.Organizaciones.Add(new Organizacion { Organizacion_Id = 8, Nombre = "Org", Tipo_Organizacion = "Tipo", Responsable_Cedula = "123", Estado_Validacion = "pendiente" });
            context.OrganizacionIdentidades.Add(new OrganizacionIdentidad
            {
                OrganizacionId = 8,
                ClientDid = "did:key:wallet",
                HolderConnectionId = "holder-connection",
                Estado = "credential_pending"
            });
            await context.SaveChangesAsync();
            SetWalletClaims(controller, 8);
            identus.Setup(x => x.GetConnectionStatusAsync("holder-connection", It.IsAny<CancellationToken>())).ReturnsAsync(new IdentusConnectionStatus
            {
                ConnectionId = "holder-connection",
                MyDid = "did:peer:cloud-representation",
                TheirDid = "did:key:serialized-wallet-representation",
                State = state
            });

            var established = await controller.EstadoConexionHolder();

            established.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(new { connected = true, holderConnectionId = "holder-connection" });
        }

        [Theory]
        [InlineData("ConnectionRequestReceived")]
        public async Task EstadoConexionHolder_ShouldRejectNonTerminalConnection(string state)
        {
            var (context, controller) = GetControllerWithIdentus(out var identus);
            context.Organizaciones.Add(new Organizacion { Organizacion_Id = 8, Nombre = "Org", Tipo_Organizacion = "Tipo", Responsable_Cedula = "123", Estado_Validacion = "pendiente" });
            context.OrganizacionIdentidades.Add(new OrganizacionIdentidad
            {
                OrganizacionId = 8,
                ClientDid = "did:key:wallet",
                HolderConnectionId = "holder-connection",
                Estado = "credential_pending"
            });
            await context.SaveChangesAsync();
            SetWalletClaims(controller, 8);
            identus.Setup(x => x.GetConnectionStatusAsync("holder-connection", It.IsAny<CancellationToken>())).ReturnsAsync(new IdentusConnectionStatus
            {
                ConnectionId = "holder-connection",
                TheirDid = "did:key:serialized-wallet-representation",
                State = state
            });

            var result = await controller.EstadoConexionHolder();

            result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(new { connected = false, holderConnectionId = "holder-connection" });
        }

        [Fact]
        public async Task RecuperarConexionHolder_ShouldReplacePendingOfferConnectionButNotCredentialRecord()
        {
            var (context, controller) = GetControllerWithIdentus(out var identus);
            context.Organizaciones.Add(new Organizacion { Organizacion_Id = 8, Nombre = "Org", Tipo_Organizacion = "Tipo", Responsable_Cedula = "123", Estado_Validacion = "pendiente" });
            context.OrganizacionIdentidades.Add(new OrganizacionIdentidad
            {
                OrganizacionId = 8, ClientDid = "did:key:wallet", HolderConnectionId = "stale",
                CredentialRecordId = "old-record", CredentialStatus = "offer_pending_holder_acceptance",
                Estado = "credential_pending", UpdatedAt = DateTime.UtcNow.AddMinutes(-2)
            });
            await context.SaveChangesAsync();
            SetWalletClaims(controller, 8);
            identus.Setup(x => x.GetConnectionStatusAsync("stale", It.IsAny<CancellationToken>())).ReturnsAsync((IdentusConnectionStatus?)null);
            identus.Setup(x => x.CreateInvitationAsync(It.IsAny<string>())).ReturnsAsync(("https://identus.test/oob", "new-connection"));

            var result = await controller.RecuperarConexionHolder();

            result.Should().BeOfType<OkObjectResult>();
            var identity = await context.OrganizacionIdentidades.SingleAsync();
            identity.ClientDid.Should().Be("did:key:wallet");
            identity.HolderConnectionId.Should().Be("new-connection");
            identity.CredentialRecordId.Should().Be("old-record");
        }

        [Theory]
        [InlineData("accepted")]
        [InlineData("issued")]
        [InlineData("completed")]
        public async Task AsociarConexion_ShouldRejectReplacementForTerminalCredentialStatus(string credentialStatus)
        {
            var (context, controller) = GetController();
            context.Organizaciones.Add(new Organizacion { Organizacion_Id = 8, Nombre = "Org", Tipo_Organizacion = "Tipo", Responsable_Cedula = "123", Estado_Validacion = "pendiente" });
            context.OrganizacionIdentidades.Add(new OrganizacionIdentidad
            {
                OrganizacionId = 8,
                HolderConnectionId = "connection-old",
                CredentialRecordId = "record-1",
                CredentialStatus = credentialStatus,
                Estado = "credential_pending"
            });
            await context.SaveChangesAsync();
            SetWalletClaims(controller, 8);

            var result = await controller.AsociarConexion(new HolderConnectionRequest { HolderConnectionId = "connection-new" });

            result.Should().BeOfType<ConflictObjectResult>();
            (await context.OrganizacionIdentidades.SingleAsync()).HolderConnectionId.Should().Be("connection-old");
        }

        private static void SetWalletClaims(OrganizacionController controller, int organizationId)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim("organizacion_id", organizationId.ToString()),
                        new Claim("rol", "2")
                    }))
                }
            };
        }

        private (EulaliaContext, OrganizacionController) GetControllerWithIdentus(out Mock<IIdentusClient> identus)
        {
            var options = new DbContextOptionsBuilder<EulaliaContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new EulaliaContext(options);
            identus = new Mock<IIdentusClient>();
            var controller = new OrganizacionController(
                new OrganizacionService(new Repository<Organizacion>(context)),
                new Repository<OrganizacionIdentidad>(context),
                identus.Object);
            return (context, controller);
        }
    }
}
