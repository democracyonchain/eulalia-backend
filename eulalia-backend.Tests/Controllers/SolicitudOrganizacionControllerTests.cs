using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using eulalia_backend.Api.Controllers;
using eulalia_backend.Domain.EntitiesRequest;
using eulalia_backend.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace eulalia_backend.Tests
{
    public class SolicitudOrganizacionControllerTests
    {
        private EulaliaContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<EulaliaContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new EulaliaContext(options);
        }

        [Theory]
        [MemberData(nameof(SolicitudTestData))]
        public async Task Crear_Solicitud_Valida_Deberia_CrearTodoCorrectamente(SolicitudOrganizacionRequest request)
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new SolicitudOrganizacionController(context);

            // Act
            var result = await controller.Crear(request) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            var value = result!.Value!;
            var messageProp = value.GetType().GetProperty("message");
            var message = messageProp?.GetValue(value)?.ToString();
            message.Should().Contain("✅");


            var ciudadano = await context.Ciudadanos.FirstOrDefaultAsync(c => c.Cedula == request.Responsable_Cedula);
            ciudadano.Should().NotBeNull();
            ciudadano!.Nombre.Should().Be(request.Responsable_Nombre);

            var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Correo == request.Responsable_Email);
            usuario.Should().NotBeNull();
            usuario!.Cedula_Ciudadano.Should().Be(request.Responsable_Cedula);

            var organizacion = await context.Organizaciones.FirstOrDefaultAsync(o => o.Nombre == request.Nombre);
            organizacion.Should().NotBeNull();
            organizacion!.Estado_Validacion.Should().Be("pendiente");

            var solicitud = await context.SolicitudOrganizacion
                .FirstOrDefaultAsync(s => s.Organizacion_Id == organizacion.Organizacion_Id);
            solicitud.Should().NotBeNull();
            solicitud!.Estado.Should().Be("pendiente");
        }

        public static IEnumerable<object[]> SolicitudTestData()
        {
            var nombres = new[] { "Luis", "Ana", "Carlos", "Lucía", "Miguel", "María", "Jorge", "Diana", "Pedro", "Sofía" };
            var apellidos = new[] { "Pérez", "Rodríguez", "Gómez", "Castro", "Vargas", "Silva", "Ortega", "Martínez", "Sánchez", "Reyes" };
            var random = new Random();

            for (int i = 1; i <= 30; i++)
            {
                var nombre = nombres[random.Next(nombres.Length)];
                var apellido = apellidos[random.Next(apellidos.Length)];
                var cedula = i.ToString("D10");
                var fechaNacimiento = new DateTime(1980 + i % 40, (i % 12) + 1, (i % 28) + 1);
                var telefono = $"09{random.Next(10000000, 99999999)}";
                var email = $"{nombre.ToLower()}.{apellido.ToLower()}{i}@movimiento.org";
                var direccion = $"Calle {i} #{random.Next(100, 999)}";

                var solicitud = new SolicitudOrganizacionRequest
                {
                    Nombre = $"Movimiento {nombre} {i}",
                    Tipo_Organizacion = "Partido Político",
                    Codigo_Provincia = random.Next(1, 25),
                    Codigo_Canton = random.Next(1, 100),
                    Codigo_Parroquia = random.Next(1000, 9999),
                    Responsable_Cedula = cedula,
                    Responsable_Nombre = nombre,
                    Responsable_Apellido = apellido,
                    Responsable_FechaNacimiento = fechaNacimiento,
                    Responsable_Direccion = direccion,
                    Responsable_Telefono = telefono,
                    Responsable_Email = email,
                    Observaciones = $"Observación de la solicitud {i}"
                };

                yield return new object[] { solicitud };
            }
        }
    }
}
