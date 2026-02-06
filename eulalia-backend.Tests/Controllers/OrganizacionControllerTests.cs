using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            var controller = new OrganizacionController(service);
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
    }
}
