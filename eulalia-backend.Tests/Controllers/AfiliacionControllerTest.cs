using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eulalia_backend.Api.Controllers;
using eulalia_backend.Application.DTOs;
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
    public class AfiliacionControllerTests
    {
        private EulaliaContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<EulaliaContext>()              
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new EulaliaContext(options);
        }

        [Fact]
        public async Task Create_ShouldAddAfiliacion()
        {
            var context = GetInMemoryContext();
            var repo = new Repository<Afiliacion>(context);
            var service = new AfiliacionService(repo);
            var controller = new AfiliacionController(service);

            var nuevaAfiliacion = new AfiliacionDto
            {
                Cedula = "1234567890",
                OrganizacionId = 1,
                FechaAfiliacion = DateTime.UtcNow,
                Estado = "activa"
            };

            var result = await controller.Create(nuevaAfiliacion);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Afiliaciones.ToListAsync();
            all.Should().ContainSingle(a => a.Cedula == "1234567890");
        }

        [Fact]
        public async Task GetAll_ShouldReturnAll()
        {
            var context = GetInMemoryContext();
            context.Afiliaciones.AddRange(new List<Afiliacion> {
                new Afiliacion { Cedula = "1", OrganizacionId = 1, Estado = "activa", FechaAfiliacion = DateTime.UtcNow },
                new Afiliacion { Cedula = "2", OrganizacionId = 2, Estado = "activa", FechaAfiliacion = DateTime.UtcNow }
            });
            await context.SaveChangesAsync();

            var repo = new Repository<Afiliacion>(context);
            var service = new AfiliacionService(repo);
            var controller = new AfiliacionController(service);

            var result = await controller.GetAll();

            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var items = okResult.Value as IEnumerable<AfiliacionDto>;
            items.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetById_ShouldReturnCorrectAfiliacion()
        {
            var context = GetInMemoryContext();
            var afiliacion = new Afiliacion { Cedula = "999", OrganizacionId = 99, Estado = "activa", FechaAfiliacion = DateTime.UtcNow };
            context.Afiliaciones.Add(afiliacion);
            await context.SaveChangesAsync();

            var repo = new Repository<Afiliacion>(context);
            var service = new AfiliacionService(repo);
            var controller = new AfiliacionController(service);

            var result = await controller.GetById(afiliacion.Afiliacion_Id);

            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var item = okResult.Value as AfiliacionDto;
            item.Should().NotBeNull();
            item!.Cedula.Should().Be("999");
        }

        [Fact]
        public async Task Anular_ShouldSetEstadoToAnulado()
        {
            var context = GetInMemoryContext();
            var afiliacion = new Afiliacion { Cedula = "mod", OrganizacionId = 1, Estado = "activa", FechaAfiliacion = DateTime.UtcNow };
            context.Afiliaciones.Add(afiliacion);
            await context.SaveChangesAsync();

            var repo = new Repository<Afiliacion>(context);
            var service = new AfiliacionService(repo);
            var controller = new AfiliacionController(service);

            var result = await controller.AnularAfiliacion(afiliacion.Afiliacion_Id);
            result.Should().BeOfType<NoContentResult>();

            var dbAfiliacion = await context.Afiliaciones.FindAsync(afiliacion.Afiliacion_Id);
            dbAfiliacion.Estado.Should().Be("Anulado");
        }
    }
}