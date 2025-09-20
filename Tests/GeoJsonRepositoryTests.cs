using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using System.Linq;
using Xunit;

namespace api.coleta.Tests;

public class GeoJsonRepositoryTests
{
    [Fact]
    public void Adicionar_DevePersistirGeojsonERetornarInstancia()
    {
        using var context = TestHelper.CreateInMemoryContext();
        var repository = new GeoJsonRepository(context);

        var geojson = new Geojson
        {
            Pontos = "{}",
            Grid = "{}"
        };

        var resultado = repository.Adicionar(geojson);

        Assert.NotNull(resultado);
        Assert.Equal(geojson.Id, resultado.Id);
        Assert.Equal(1, context.Geojson.Count());
    }

    [Fact]
    public void ObterPorId_DeveRetornarGeojsonQuandoExistente()
    {
        using var context = TestHelper.CreateInMemoryContext();
        var repository = new GeoJsonRepository(context);

        var geojson = new Geojson
        {
            Pontos = "{\"data\":1}",
            Grid = "{\"grid\":[1,2]}"
        };

        repository.Adicionar(geojson);

        var resultado = repository.ObterPorId(geojson.Id);

        Assert.NotNull(resultado);
        Assert.Equal(geojson.Pontos, resultado!.Pontos);
        Assert.Equal(geojson.Grid, resultado.Grid);
    }
}
