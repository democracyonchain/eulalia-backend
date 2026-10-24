using eulalia_backend.Domain.Entities;
using eulalia_backend.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace eulalia_backend.Tests.Infrastructure;

public sealed class EulaliaContextTests
{
    [Fact]
    public void AfiliacionCedulaIndex_IsUniqueForNonAnnulledRows()
    {
        var options = new DbContextOptionsBuilder<EulaliaContext>()
            .UseNpgsql("Host=unused;Database=unused")
            .Options;

        using var context = new EulaliaContext(options);
        var index = context.Model
            .FindEntityType(typeof(Afiliacion))!
            .GetIndexes()
            .Single(index => index.GetDatabaseName() == "ux_afiliacion_cedula_active");

        index.IsUnique.Should().BeTrue();
        index.GetFilter().Should().Be("estado <> 'Anulado'");
    }
}
