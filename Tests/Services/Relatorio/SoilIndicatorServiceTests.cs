using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Services.Relatorio;
using System.Text.Json;
using Xunit;

namespace api.coleta.Tests.Services.Relatorio;

/// <summary>
/// Testes unitários para o SoilIndicatorService
/// </summary>
public class SoilIndicatorServiceTests
{
    private readonly SoilIndicatorService _service;
    private readonly NutrientClassificationService _classificationService;

    public SoilIndicatorServiceTests()
    {
        _classificationService = new NutrientClassificationService();
        _service = new SoilIndicatorService(_classificationService);
    }

    [Fact]
    public void CalcularIndicador_DeveRetornarSemDadosQuandoContadorZero()
    {
        // Arrange
        var configsPersonalizadas = new Dictionary<string, NutrientConfig>();

        // Act
        var resultado = _service.CalcularIndicador("pH", 0, 0, configsPersonalizadas);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(0, resultado.ValorMedio);
        Assert.Equal("Sem dados", resultado.Classificacao);
        Assert.Equal("#CCCCCC", resultado.Cor);
    }

    [Fact]
    public void CalcularIndicador_DeveCalcularMediaCorretamente()
    {
        // Arrange
        var configsPersonalizadas = new Dictionary<string, NutrientConfig>();
        double soma = 15.0;
        int contador = 3;

        // Act
        var resultado = _service.CalcularIndicador("pH", soma, contador, configsPersonalizadas);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(5.0, resultado.ValorMedio);
    }

    [Fact]
    public void CalcularIndicadorComReferencia_DeveRetornarSemDadosQuandoContadorZero()
    {
        // Arrange
        var configsPersonalizadas = new Dictionary<string, NutrientConfig>();

        // Act
        var resultado = _service.CalcularIndicadorComReferencia(
            "Ca", 0, 0, configsPersonalizadas, 10.0, "CTC");

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(0, resultado.ValorMedio);
        Assert.Equal("Sem dados", resultado.Classificacao);
        Assert.Equal("#CCCCCC", resultado.Cor);
    }

    [Fact]
    public void CalcularIndicadorComReferencia_DeveCalcularMediaComDuasCasasDecimais()
    {
        // Arrange
        var configsPersonalizadas = new Dictionary<string, NutrientConfig>();
        double soma = 7.777;
        int contador = 1;

        // Act
        var resultado = _service.CalcularIndicadorComReferencia(
            "Ca", soma, contador, configsPersonalizadas, 10.0, "CTC");

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(7.78, resultado.ValorMedio);
    }

    [Fact]
    public void BuscarEAcumularValor_DeveAcumularQuandoChaveExiste()
    {
        // Arrange
        var jsonString = "{\"pH\": 5.5, \"Ca\": 3.2}";
        var ponto = JsonSerializer.Deserialize<JsonElement>(jsonString);
        var chaves = new[] { "pH", "pH (CaCl2)" };
        double soma = 0;
        int contador = 0;

        // Act
        _service.BuscarEAcumularValor(ponto, chaves, ref soma, ref contador);

        // Assert
        Assert.Equal(5.5, soma);
        Assert.Equal(1, contador);
    }

    [Fact]
    public void BuscarEAcumularValor_NaoDeveAcumularQuandoChaveNaoExiste()
    {
        // Arrange
        var jsonString = "{\"Ca\": 3.2}";
        var ponto = JsonSerializer.Deserialize<JsonElement>(jsonString);
        var chaves = new[] { "pH", "pH (CaCl2)" };
        double soma = 0;
        int contador = 0;

        // Act
        _service.BuscarEAcumularValor(ponto, chaves, ref soma, ref contador);

        // Assert
        Assert.Equal(0, soma);
        Assert.Equal(0, contador);
    }

    [Fact]
    public void BuscarEAcumularValor_DeveUsarPrimeiraChaveEncontrada()
    {
        // Arrange
        var jsonString = "{\"pH (CaCl2)\": 5.8, \"pH\": 5.5}";
        var ponto = JsonSerializer.Deserialize<JsonElement>(jsonString);
        var chaves = new[] { "pH (CaCl2)", "pH" };
        double soma = 0;
        int contador = 0;

        // Act
        _service.BuscarEAcumularValor(ponto, chaves, ref soma, ref contador);

        // Assert
        Assert.Equal(5.8, soma);
        Assert.Equal(1, contador);
    }
}
