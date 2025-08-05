
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

        [Fact]
        public async Task Escenario_01_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "7211922022",
                Nombre = "Jorge",
                Apellido = "Torres Silva",
                Direccion = "Centro",
                Telefono = "0948572783",
                Fecha_Nacimiento = new DateTime(1990, 12, 10)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "7211922022");
        }


        [Fact]
        public async Task Escenario_02_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "0197711525",
                Nombre = "Laura",
                Apellido = "Rodríguez",
                Direccion = "Sur",
                Telefono = "0948329308",
                Fecha_Nacimiento = new DateTime(2000, 8, 22)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "0197711525");
        }


        [Fact]
        public async Task Escenario_03_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "3070591299",
                Nombre = "Andrés Sofía",
                Apellido = "Cruz Martínez",
                Direccion = "Centro",
                Telefono = "0998175456",
                Fecha_Nacimiento = new DateTime(1982, 9, 1)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "3070591299");
        }


        [Fact]
        public async Task Escenario_04_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "0686337027",
                Nombre = "José",
                Apellido = "Guerrero Mendoza",
                Direccion = "Centro",
                Telefono = "0938413930",
                Fecha_Nacimiento = new DateTime(2003, 9, 17)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "0686337027");
        }


        [Fact]
        public async Task Escenario_05_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "0306010688",
                Nombre = "Ana Pedro",
                Apellido = "Ortega",
                Direccion = "Este",
                Telefono = "0960744756",
                Fecha_Nacimiento = new DateTime(1992, 8, 23)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "0306010688");
        }


        [Fact]
        public async Task Escenario_06_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "0503520056",
                Nombre = "Ana",
                Apellido = "Vargas",
                Direccion = "Sur",
                Telefono = "0961496826",
                Fecha_Nacimiento = new DateTime(1976, 12, 1)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "0503520056");
        }


        [Fact]
        public async Task Escenario_07_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "6692510654",
                Nombre = "Pedro",
                Apellido = "Silva",
                Direccion = "Este",
                Telefono = "0938624387",
                Fecha_Nacimiento = new DateTime(2004, 11, 3)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "6692510654");
        }


        [Fact]
        public async Task Escenario_08_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "5202058939",
                Nombre = "Laura Ana",
                Apellido = "Silva",
                Direccion = "Este",
                Telefono = "0918709273",
                Fecha_Nacimiento = new DateTime(1992, 2, 7)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "5202058939");
        }


        [Fact]
        public async Task Escenario_09_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "0682669909",
                Nombre = "María",
                Apellido = "Torres Vargas",
                Direccion = "Oeste",
                Telefono = "0915511643",
                Fecha_Nacimiento = new DateTime(1993, 8, 27)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "0682669909");
        }


        [Fact]
        public async Task Escenario_10_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "0156114874",
                Nombre = "David Ana",
                Apellido = "Guerrero Castro",
                Direccion = "Sur",
                Telefono = "0989450129",
                Fecha_Nacimiento = new DateTime(1991, 5, 15)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "0156114874");
        }


        [Fact]
        public async Task Escenario_11_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "7664009348",
                Nombre = "Andrés Diana",
                Apellido = "Flores Silva",
                Direccion = "Oeste",
                Telefono = "0969411940",
                Fecha_Nacimiento = new DateTime(1987, 10, 27)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "7664009348");
        }


        [Fact]
        public async Task Escenario_12_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "1239923135",
                Nombre = "Carlos Luis",
                Apellido = "Vargas",
                Direccion = "Sur",
                Telefono = "0975096432",
                Fecha_Nacimiento = new DateTime(1988, 10, 14)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "1239923135");
        }


        [Fact]
        public async Task Escenario_13_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "9348547948",
                Nombre = "Carmen",
                Apellido = "Martínez Mendoza",
                Direccion = "Sur",
                Telefono = "0944497749",
                Fecha_Nacimiento = new DateTime(1991, 3, 7)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "9348547948");
        }


        [Fact]
        public async Task Escenario_14_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "7090463776",
                Nombre = "Claudia Lucía",
                Apellido = "Vargas",
                Direccion = "Oeste",
                Telefono = "0942360425",
                Fecha_Nacimiento = new DateTime(1979, 9, 9)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "7090463776");
        }


        [Fact]
        public async Task Escenario_15_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "4666428908",
                Nombre = "Laura",
                Apellido = "Silva",
                Direccion = "Centro",
                Telefono = "0991750845",
                Fecha_Nacimiento = new DateTime(1998, 8, 8)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "4666428908");
        }


        [Fact]
        public async Task Escenario_16_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "0078320721",
                Nombre = "Miguel Luis",
                Apellido = "Silva Rodríguez",
                Direccion = "Norte",
                Telefono = "0912237350",
                Fecha_Nacimiento = new DateTime(1979, 7, 1)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "0078320721");
        }


        [Fact]
        public async Task Escenario_17_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "5132723103",
                Nombre = "Laura",
                Apellido = "Torres Castro",
                Direccion = "Norte",
                Telefono = "0942236621",
                Fecha_Nacimiento = new DateTime(1997, 3, 13)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "5132723103");
        }


        [Fact]
        public async Task Escenario_18_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "6044402636",
                Nombre = "Juan",
                Apellido = "Flores Pérez",
                Direccion = "Sur",
                Telefono = "0945615682",
                Fecha_Nacimiento = new DateTime(1998, 11, 9)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "6044402636");
        }


        [Fact]
        public async Task Escenario_19_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "3892026804",
                Nombre = "Diana Diana",
                Apellido = "Rodríguez Mendoza",
                Direccion = "Este",
                Telefono = "0928690068",
                Fecha_Nacimiento = new DateTime(1990, 12, 26)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "3892026804");
        }


        [Fact]
        public async Task Escenario_20_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "7747527457",
                Nombre = "Juan",
                Apellido = "Sánchez",
                Direccion = "Norte",
                Telefono = "0925025128",
                Fecha_Nacimiento = new DateTime(1975, 11, 17)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "7747527457");
        }


        [Fact]
        public async Task Escenario_21_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "2285368737",
                Nombre = "Claudia",
                Apellido = "Rodríguez",
                Direccion = "Centro",
                Telefono = "0950710102",
                Fecha_Nacimiento = new DateTime(1977, 3, 15)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "2285368737");
        }


        [Fact]
        public async Task Escenario_22_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "6627167516",
                Nombre = "Carmen Carmen",
                Apellido = "Castro Rivera",
                Direccion = "Oeste",
                Telefono = "0959188569",
                Fecha_Nacimiento = new DateTime(1970, 6, 9)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "6627167516");
        }


        [Fact]
        public async Task Escenario_23_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "0680505021",
                Nombre = "Pedro David",
                Apellido = "Pérez",
                Direccion = "Oeste",
                Telefono = "0988512395",
                Fecha_Nacimiento = new DateTime(1994, 9, 12)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "0680505021");
        }


        [Fact]
        public async Task Escenario_24_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "1239575449",
                Nombre = "Carlos",
                Apellido = "Rivera Mendoza",
                Direccion = "Centro",
                Telefono = "0940043956",
                Fecha_Nacimiento = new DateTime(1997, 3, 11)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "1239575449");
        }


        [Fact]
        public async Task Escenario_25_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "1890933718",
                Nombre = "Daniel",
                Apellido = "Gómez",
                Direccion = "Sur",
                Telefono = "0971919025",
                Fecha_Nacimiento = new DateTime(2001, 4, 10)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "1890933718");
        }


        [Fact]
        public async Task Escenario_26_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "4602640634",
                Nombre = "Verónica Diana",
                Apellido = "Rodríguez Rodríguez",
                Direccion = "Norte",
                Telefono = "0927256753",
                Fecha_Nacimiento = new DateTime(1970, 8, 18)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "4602640634");
        }


        [Fact]
        public async Task Escenario_27_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "7749104165",
                Nombre = "Luis Daniel",
                Apellido = "Rodríguez Reyes",
                Direccion = "Sur",
                Telefono = "0977154903",
                Fecha_Nacimiento = new DateTime(2001, 8, 11)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "7749104165");
        }


        [Fact]
        public async Task Escenario_28_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "2133682695",
                Nombre = "Jorge Daniel",
                Apellido = "Castro Reyes",
                Direccion = "Oeste",
                Telefono = "0970944261",
                Fecha_Nacimiento = new DateTime(1988, 7, 10)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "2133682695");
        }


        [Fact]
        public async Task Escenario_29_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "2011475923",
                Nombre = "Laura",
                Apellido = "Cruz Sánchez",
                Direccion = "Centro",
                Telefono = "0984710713",
                Fecha_Nacimiento = new DateTime(1995, 4, 6)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "2011475923");
        }


        [Fact]
        public async Task Escenario_30_CrearCiudadano_Valido()
        {
            var context = GetInMemoryContext();
            var controller = new CiudadanoController(context);

            var ciudadano = new Ciudadano
            {
                Cedula = "4646475800",
                Nombre = "Juan",
                Apellido = "Sánchez",
                Direccion = "Este",
                Telefono = "0916934004",
                Fecha_Nacimiento = new DateTime(2003, 9, 13)
            };

            var result = await controller.Create(ciudadano);
            result.Result.Should().BeOfType<CreatedAtActionResult>();

            var all = await context.Ciudadanos.ToListAsync();
            all.Should().ContainSingle(c => c.Cedula == "4646475800");
        }

    }
}
