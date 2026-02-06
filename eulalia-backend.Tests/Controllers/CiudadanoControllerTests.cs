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

namespace eulalia_backend.Tests
{
    public class CiudadanoControllerDynamicTests
    {
        private EulaliaContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<EulaliaContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new EulaliaContext(options);
        }

        [Theory]
        [MemberData(nameof(GetCiudadanos))]
        public async Task CrearCiudadano_Valido(CiudadanoDto dto)
        {
            var context = GetInMemoryContext();
            var repo = new Repository<Ciudadano>(context);
            var service = new CiudadanoService(repo);
            var controller = new CiudadanoController(service);

            var result = await controller.Create(dto);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == dto.Cedula);
        }

        public static IEnumerable<object[]> GetCiudadanos()
        {
            var testCases = new List<object[]>();
            
            // Generar 30 casos de prueba similares a los originales
            var cedulas = new[] { "7211922022", "0197711525", "3070591299", "0686337027", "0306010688" }; // Muestra
            var nombres = new[] { "Jorge", "Laura", "Andrés", "José", "Ana" };
            var apellidos = new[] { "Torres", "Rodríguez", "Cruz", "Guerrero", "Ortega" };
            var random = new Random(123); // Seed fijo para consistencia

            for (int i = 0; i < 30; i++)
            {
                var dto = new CiudadanoDto
                {
                    Cedula = i < cedulas.Length ? cedulas[i] : random.Next(100000000, 999999999).ToString("D10"),
                    Nombres = nombres[i % nombres.Length],
                    Apellidos = apellidos[i % apellidos.Length],
                    Telefono = $"09{random.Next(10000000, 99999999)}",
                    FechaNacimiento = new DateTime(1980 + (i % 20), (i % 12) + 1, (i % 28) + 1)
                };
                testCases.Add(new object[] { dto });
            }

            return testCases;
        }
    }
}
