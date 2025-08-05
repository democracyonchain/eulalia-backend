using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eulalia_backend.Api.Controllers;
using eulalia_backend.Domain.Entities;
using eulalia_backend.Infrastructure.Data;
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
            var controller = new AfiliacionController(context);

            var nuevaAfiliacion = new Afiliacion
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
                new Afiliacion { Cedula = "1", OrganizacionId = 1 },
                new Afiliacion { Cedula = "2", OrganizacionId = 2 }
            });
            await context.SaveChangesAsync();

            var controller = new AfiliacionController(context);
            var result = await controller.GetAll();

            result.Value.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetById_ShouldReturnCorrectAfiliacion()
        {
            var context = GetInMemoryContext();
            var afiliacion = new Afiliacion { Cedula = "999", OrganizacionId = 99 };
            context.Afiliaciones.Add(afiliacion);
            await context.SaveChangesAsync();

            var controller = new AfiliacionController(context);
            var result = await controller.GetById(afiliacion.Afiliacion_Id);

            result.Value.Should().NotBeNull();
            result.Value.Cedula.Should().Be("999");
        }

        [Fact]
        public async Task Update_ShouldModifyAfiliacion()
        {
            var context = GetInMemoryContext();
            var afiliacion = new Afiliacion { Cedula = "mod", OrganizacionId = 1 };
            context.Afiliaciones.Add(afiliacion);
            await context.SaveChangesAsync();

            var controller = new AfiliacionController(context);
            afiliacion.Cedula = "modificado";

            var result = await controller.Update(afiliacion.Afiliacion_Id, afiliacion);
            result.Should().BeOfType<NoContentResult>();

            var dbAfiliacion = await context.Afiliaciones.FindAsync(afiliacion.Afiliacion_Id);
            dbAfiliacion.Cedula.Should().Be("modificado");
        }

        [Fact]
        public async Task Delete_ShouldRemoveAfiliacion()
        {
            var context = GetInMemoryContext();
            var afiliacion = new Afiliacion { Cedula = "del", OrganizacionId = 1 };
            context.Afiliaciones.Add(afiliacion);
            await context.SaveChangesAsync();

            var controller = new AfiliacionController(context);
            var result = await controller.Delete(afiliacion.Afiliacion_Id);

            result.Should().BeOfType<NoContentResult>();
            (await context.Afiliaciones.FindAsync(afiliacion.Afiliacion_Id)).Should().BeNull();
        }
    }
}