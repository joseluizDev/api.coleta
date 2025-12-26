using api.coleta.Services.Relatorio;
using api.coleta.Repositories;
using api.coleta.Tests.Helpers;
using Xunit;

namespace api.coleta.Tests.Services.Relatorio;

/// <summary>
/// Testes unitários para o GeoJsonProcessorService
/// </summary>
public class GeoJsonProcessorServiceTests
{
    private GeoJsonProcessorService CreateService()
    {
        var context = TestHelper.CreateInMemoryContext();
        var repository = new GeoJsonRepository(context);
        return new GeoJsonProcessorService(repository);
    }

    [Fact]
    public void ProcessarGeoJsonString_DeveRetornarNullQuandoJsonNulo()
    {
        // Arrange
        var service = CreateService();

        // Act
        var resultado = service.ProcessarGeoJsonString(null, out int zonas);

        // Assert
        Assert.Null(resultado);
        Assert.Equal(0, zonas);
    }

    [Fact]
    public void ProcessarGeoJsonString_DeveRetornarNullQuandoJsonVazio()
    {
        // Arrange
        var service = CreateService();

        // Act
        var resultado = service.ProcessarGeoJsonString("", out int zonas);

        // Assert
        Assert.Null(resultado);
        Assert.Equal(0, zonas);
    }

    [Fact]
    public void ProcessarGeoJsonString_DeveRetornarNullQuandoJsonInvalido()
    {
        // Arrange
        var service = CreateService();
        var jsonInvalido = "{ invalid json }";

        // Act
        var resultado = service.ProcessarGeoJsonString(jsonInvalido, out int zonas);

        // Assert
        Assert.Null(resultado);
        Assert.Equal(0, zonas);
    }

    [Fact]
    public void ProcessarGeoJsonString_DeveExtrairGridDePolygons()
    {
        // Arrange
        var service = CreateService();
        var geoJson = @"{
            ""type"": ""FeatureCollection"",
            ""features"": [
                {
                    ""type"": ""Feature"",
                    ""geometry"": {
                        ""type"": ""Polygon"",
                        ""coordinates"": [[[-46.5, -23.5], [-46.4, -23.5], [-46.4, -23.4], [-46.5, -23.4], [-46.5, -23.5]]]
                    },
                    ""properties"": {}
                },
                {
                    ""type"": ""Feature"",
                    ""geometry"": {
                        ""type"": ""Polygon"",
                        ""coordinates"": [[[-46.3, -23.3], [-46.2, -23.3], [-46.2, -23.2], [-46.3, -23.2], [-46.3, -23.3]]]
                    },
                    ""properties"": {}
                }
            ]
        }";

        // Act
        var resultado = service.ProcessarGeoJsonString(geoJson, out int zonas);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(2, zonas);
    }

    [Fact]
    public void ProcessarGeoJsonString_DeveExtrairPontos()
    {
        // Arrange
        var service = CreateService();
        var geoJson = @"{
            ""type"": ""FeatureCollection"",
            ""features"": [],
            ""points"": [
                {
                    ""type"": ""Feature"",
                    ""geometry"": {
                        ""type"": ""Point"",
                        ""coordinates"": [-46.5, -23.5]
                    },
                    ""properties"": {
                        ""id"": 1,
                        ""hexagonId"": 1,
                        ""coletado"": true
                    }
                },
                {
                    ""type"": ""Feature"",
                    ""geometry"": {
                        ""type"": ""Point"",
                        ""coordinates"": [-46.4, -23.4]
                    },
                    ""properties"": {
                        ""id"": 2,
                        ""hexagonId"": 2,
                        ""coletado"": false
                    }
                }
            ]
        }";

        // Act
        var resultado = service.ProcessarGeoJsonString(geoJson, out int zonas);

        // Assert
        Assert.NotNull(resultado);
        // Verifica que o resultado é um objeto com grid e points
        var resultType = resultado.GetType();
        Assert.NotNull(resultType.GetProperty("grid"));
        Assert.NotNull(resultType.GetProperty("points"));
    }

    [Fact]
    public void ProcessarGeoJsonString_DeveIgnorarFeaturesNaoPolygon()
    {
        // Arrange
        var service = CreateService();
        var geoJson = @"{
            ""type"": ""FeatureCollection"",
            ""features"": [
                {
                    ""type"": ""Feature"",
                    ""geometry"": {
                        ""type"": ""Point"",
                        ""coordinates"": [-46.5, -23.5]
                    },
                    ""properties"": {}
                },
                {
                    ""type"": ""Feature"",
                    ""geometry"": {
                        ""type"": ""LineString"",
                        ""coordinates"": [[-46.5, -23.5], [-46.4, -23.4]]
                    },
                    ""properties"": {}
                },
                {
                    ""type"": ""Feature"",
                    ""geometry"": {
                        ""type"": ""Polygon"",
                        ""coordinates"": [[[-46.5, -23.5], [-46.4, -23.5], [-46.4, -23.4], [-46.5, -23.4], [-46.5, -23.5]]]
                    },
                    ""properties"": {}
                }
            ]
        }";

        // Act
        var resultado = service.ProcessarGeoJsonString(geoJson, out int zonas);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(1, zonas); // Apenas o Polygon deve ser contado
    }

    [Fact]
    public void ProcessarGeoJsonString_DeveRetornarGridVazioQuandoSemFeatures()
    {
        // Arrange
        var service = CreateService();
        var geoJson = @"{
            ""type"": ""FeatureCollection""
        }";

        // Act
        var resultado = service.ProcessarGeoJsonString(geoJson, out int zonas);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(0, zonas);
    }

    [Fact]
    public void ProcessarGeoJson_DeveRetornarNullQuandoIdNaoExiste()
    {
        // Arrange
        var service = CreateService();
        var idInexistente = Guid.NewGuid();

        // Act
        var resultado = service.ProcessarGeoJson(idInexistente, out int zonas);

        // Assert
        Assert.Null(resultado);
        Assert.Equal(0, zonas);
    }
}
