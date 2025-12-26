using api.coleta.Models.Entidades;
using api.coleta.Services.Relatorio;
using System.Text.Json;
using Xunit;

namespace api.coleta.Tests.Services.Relatorio;

/// <summary>
/// Testes unitários para o AttributeStatisticsService
/// </summary>
public class AttributeStatisticsServiceTests
{
    private readonly AttributeStatisticsService _service;
    private readonly NutrientClassificationService _classificationService;

    public AttributeStatisticsServiceTests()
    {
        _classificationService = new NutrientClassificationService();
        _service = new AttributeStatisticsService(_classificationService);
    }

    [Fact]
    public void CalcularEstatisticaAtributo_DeveCalcularMinimoMaximoMedia()
    {
        // Arrange
        var valores = new List<double> { 5.0, 6.0, 7.0, 8.0, 9.0 };
        var configsPersonalizadas = new Dictionary<string, NutrientConfig>();

        // Act
        var resultado = _service.CalcularEstatisticaAtributo(
            "pH", valores, configsPersonalizadas, 0, 0);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("pH", resultado.Nome);
        Assert.Equal(5.0, resultado.Minimo);
        Assert.Equal(9.0, resultado.Maximo);
        Assert.Equal(7.0, resultado.Media);
        Assert.Equal(5, resultado.QuantidadePontos);
    }

    [Fact]
    public void CalcularEstatisticaAtributo_DeveArredondarParaDuasCasasDecimais()
    {
        // Arrange
        var valores = new List<double> { 5.555, 6.666, 7.777 };
        var configsPersonalizadas = new Dictionary<string, NutrientConfig>();

        // Act
        var resultado = _service.CalcularEstatisticaAtributo(
            "pH", valores, configsPersonalizadas, 0, 0);

        // Assert
        Assert.Equal(5.56, resultado.Minimo);
        Assert.Equal(7.78, resultado.Maximo);
        Assert.Equal(6.67, resultado.Media);
    }

    [Fact]
    public void CalcularEstatisticasAtributos_DeveRetornarVazioQuandoJsonNulo()
    {
        // Arrange
        var configsPersonalizadas = new Dictionary<string, NutrientConfig>();

        // Act
        var resultado = _service.CalcularEstatisticasAtributos(null, configsPersonalizadas);

        // Assert
        Assert.NotNull(resultado);
        Assert.Empty(resultado);
    }

    [Fact]
    public void CalcularEstatisticasAtributos_DeveRetornarVazioQuandoJsonVazio()
    {
        // Arrange
        var configsPersonalizadas = new Dictionary<string, NutrientConfig>();

        // Act
        var resultado = _service.CalcularEstatisticasAtributos("", configsPersonalizadas);

        // Assert
        Assert.NotNull(resultado);
        Assert.Empty(resultado);
    }

    [Fact]
    public void CalcularEstatisticasAtributos_DeveProcessarJsonComPontos()
    {
        // Arrange
        var json = @"[
            {""ID"": 1, ""pH"": 5.5, ""P"": 12.0},
            {""ID"": 2, ""pH"": 6.0, ""P"": 15.0},
            {""ID"": 3, ""pH"": 6.5, ""P"": 18.0}
        ]";
        var configsPersonalizadas = new Dictionary<string, NutrientConfig>();

        // Act
        var resultado = _service.CalcularEstatisticasAtributos(json, configsPersonalizadas);

        // Assert
        Assert.NotNull(resultado);
        Assert.True(resultado.ContainsKey("pH"));
        Assert.True(resultado.ContainsKey("P"));
        Assert.False(resultado.ContainsKey("ID")); // ID deve ser ignorado
    }

    [Fact]
    public void CalcularEstatisticasAtributos_DeveIgnorarAtributosEspecificos()
    {
        // Arrange
        var json = @"[
            {""ID"": 1, ""id"": 1, ""prof."": ""0-20"", ""profundidade"": ""0-20"", ""pH"": 5.5}
        ]";
        var configsPersonalizadas = new Dictionary<string, NutrientConfig>();

        // Act
        var resultado = _service.CalcularEstatisticasAtributos(json, configsPersonalizadas);

        // Assert
        Assert.True(resultado.ContainsKey("pH"));
        Assert.False(resultado.ContainsKey("ID"));
        Assert.False(resultado.ContainsKey("id"));
        Assert.False(resultado.ContainsKey("prof."));
        Assert.False(resultado.ContainsKey("profundidade"));
    }

    [Fact]
    public void DeterminarReferencia_DeveRetornarCTCParaAtributosDependentes()
    {
        // Arrange & Act
        var (referencia, valor) = _service.DeterminarReferencia("Ca", 10.0, 25.0);

        // Assert
        Assert.Equal("CTC", referencia);
        Assert.Equal(10.0, valor);
    }

    [Fact]
    public void DeterminarReferencia_DeveRetornarArgilaParaFosforo()
    {
        // Arrange & Act
        var (referencia, valor) = _service.DeterminarReferencia("P Mehlich", 10.0, 25.0);

        // Assert
        Assert.Equal("Argila", referencia);
        Assert.Equal(25.0, valor);
    }

    [Fact]
    public void DeterminarReferencia_DeveRetornarNullParaAtributosIndependentes()
    {
        // Arrange & Act - usando "V%" que não depende de CTC nem Argila
        var (referencia, valor) = _service.DeterminarReferencia("V%", 10.0, 25.0);

        // Assert
        Assert.Null(referencia);
        Assert.Equal(0, valor);
    }

    [Fact]
    public void CalcularEstatisticasAtributos_DeveRetornarVazioParaJsonInvalido()
    {
        // Arrange
        var jsonInvalido = "{ invalid json }";
        var configsPersonalizadas = new Dictionary<string, NutrientConfig>();

        // Act
        var resultado = _service.CalcularEstatisticasAtributos(jsonInvalido, configsPersonalizadas);

        // Assert
        Assert.NotNull(resultado);
        Assert.Empty(resultado);
    }

    [Fact]
    public void CalcularEstatisticasAtributos_DeveRetornarVazioParaJsonNaoArray()
    {
        // Arrange
        var json = @"{""pH"": 5.5}";
        var configsPersonalizadas = new Dictionary<string, NutrientConfig>();

        // Act
        var resultado = _service.CalcularEstatisticasAtributos(json, configsPersonalizadas);

        // Assert
        Assert.NotNull(resultado);
        Assert.Empty(resultado);
    }
}
