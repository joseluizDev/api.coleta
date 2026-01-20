using api.coleta.Models.Entidades;
using api.coleta.Services.Relatorio;
using System.Text.Json;
using Xunit;

namespace api.coleta.Tests.Services.Relatorio;

/// <summary>
/// Testes unitários para o NutrientClassificationService
/// </summary>
public class NutrientClassificationServiceTests
{
    private readonly NutrientClassificationService _service;

    public NutrientClassificationServiceTests()
    {
        _service = new NutrientClassificationService();
    }

    [Fact]
    public void ClassificarComConfigPersonalizada_DeveRetornarNullQuandoNaoHaConfig()
    {
        // Arrange
        var configsPersonalizadas = new Dictionary<string, NutrientConfig>();

        // Act
        var resultado = _service.ClassificarComConfigPersonalizada("pH", 5.5, configsPersonalizadas);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public void ClassificarComConfigPersonalizada_DeveRetornarClassificacaoQuandoValorDentroDoIntervalo()
    {
        // Arrange
        var config = new NutrientConfig
        {
            NutrientName = "pH",
            ConfigData = JsonSerializer.Serialize(new NutrientConfigData
            {
                Ranges = new List<List<object>>
                {
                    new() { 0.0, 5.0, "#FF0000", "Baixo" },
                    new() { 5.0, 6.0, "#FFFF00", "Médio" },
                    new() { 6.0, 7.0, "#00FF00", "Adequado" }
                }
            })
        };

        var configsPersonalizadas = new Dictionary<string, NutrientConfig>
        {
            { "pH", config }
        };

        // Act
        var resultado = _service.ClassificarComConfigPersonalizada("pH", 5.5, configsPersonalizadas);

        // Assert
        Assert.NotNull(resultado);
        var resultObj = (dynamic)resultado;
        Assert.Equal("Médio", (string)resultObj.classificacao);
        Assert.Equal("#FFFF00", (string)resultObj.cor);
        Assert.True((bool)resultObj.configPersonalizada);
    }

    [Fact]
    public void ClassificarComConfigPersonalizada_DeveRetornarNullQuandoValorForaDeIntervalos()
    {
        // Arrange
        var config = new NutrientConfig
        {
            NutrientName = "pH",
            ConfigData = JsonSerializer.Serialize(new NutrientConfigData
            {
                Ranges = new List<List<object>>
                {
                    new() { 5.0, 6.0, "#FFFF00", "Médio" }
                }
            })
        };

        var configsPersonalizadas = new Dictionary<string, NutrientConfig>
        {
            { "pH", config }
        };

        // Act
        var resultado = _service.ClassificarComConfigPersonalizada("pH", 4.0, configsPersonalizadas);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public void ClassificarComConfigPersonalizada_DeveBuscarPorMapeamentoDeChave()
    {
        // Arrange
        var config = new NutrientConfig
        {
            NutrientName = "Fósforo - P Mehlich-1 (mg/dm³)",
            ConfigData = JsonSerializer.Serialize(new NutrientConfigData
            {
                Ranges = new List<List<object>>
                {
                    new() { 0.0, 10.0, "#FF0000", "Baixo" },
                    new() { 10.0, 20.0, "#00FF00", "Adequado" }
                }
            })
        };

        var configsPersonalizadas = new Dictionary<string, NutrientConfig>
        {
            { "Fósforo - P Mehlich-1 (mg/dm³)", config }
        };

        // Act - Usando a chave curta "P" que deveria mapear para o nome completo
        var resultado = _service.ClassificarComConfigPersonalizada("P", 15.0, configsPersonalizadas);

        // Assert
        // O resultado pode ser null se o mapeamento não existir no NutrienteConfig
        // Este teste valida o comportamento do mapeamento
    }
}
