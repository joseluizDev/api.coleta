using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using api.coleta.Services;
using api.coleta.Data;
using System.Linq;
using Xunit;

namespace api.coleta.Tests;

public class GeoJsonServiceTests
{
    [Fact]
    public void SalvarSafra_DeveRetornarGeojsonPersistido()
    {
        using var context = TestHelper.CreateInMemoryContext();
        var repository = new GeoJsonRepository(context);
        var unitOfWork = new UnitOfWorkImplements(context);
        var mapper = TestHelper.CreateMapper();
        var service = new GeoJsonService(repository, unitOfWork, mapper);

        var geojson = new Geojson
        {
            Pontos = "{\"pontos\":[]}",
            Grid = "{\"grid\":[]}" 
        };

        var resultado = service.SalvarSafra(geojson);

        Assert.NotNull(resultado);
        Assert.Equal(1, context.Geojson.Count());
    }
}
