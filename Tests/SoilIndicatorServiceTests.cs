using System.Text.Json;
using api.coleta.Models.Entidades;
using api.coleta.Services.Relatorio;
using Xunit;
using Xunit.Abstractions;

namespace api.coleta.Tests;

/// <summary>
/// Testa o SoilIndicatorService focando em:
/// 1) CalcularIndicador para "Semente Esverdeada" e "Umidade"
/// 2) BuscarEAcumularValor com chaves com espaços (trailing/leading space)
/// Valida que os campos recebem classificação e cor corretas após ajuste no RelatorioService.
/// </summary>
public class SoilIndicatorServiceTests
{
    private readonly ITestOutputHelper _output;
    private readonly SoilIndicatorService _service;
    private readonly Dictionary<string, NutrientConfig> _configsVazias = new();

    // JSON com 5 amostras - mesmo dado de TesteJsonNutrientClassificationTests
    // "Semente Esverdeada " (trailing space) simula a coluna real do arquivo Excel
    private static readonly string JsonRelatorio = System.Text.Json.JsonSerializer.Serialize(new[]
    {
        new Dictionary<string, object> { ["Semente Esverdeada "] = "%", ["Umidade"] = "%" },
        new Dictionary<string, object> { ["ID da Amostra"] = 1, ["Semente Esverdeada "] = 3, ["Umidade"] = 14 },
        new Dictionary<string, object> { ["ID da Amostra"] = 2, ["Semente Esverdeada "] = 4, ["Umidade"] = 16 },
        new Dictionary<string, object> { ["ID da Amostra"] = 3, ["Semente Esverdeada "] = 2, ["Umidade"] = 11 },
        new Dictionary<string, object> { ["ID da Amostra"] = 4, ["Semente Esverdeada "] = 2, ["Umidade"] = 20 },
        new Dictionary<string, object> { ["ID da Amostra"] = 5, ["Semente Esverdeada "] = 0, ["Umidade"] = 22 },
    });

    public SoilIndicatorServiceTests(ITestOutputHelper output)
    {
        _output = output;
        var classificationService = new NutrientClassificationService();
        _service = new SoilIndicatorService(classificationService);
    }

    // ─── CalcularIndicador – "Semente Esverdeada" ──────────────────────────────

    /// <summary>
    /// Média das 5 amostras = (3+4+2+2+0)/5 = 2.2 → intervalo [2,3) = "Baixo", #90FF4C
    /// </summary>
    [Fact]
    public void SementeEsverdeada_MediaDasAmostras_DeveSerClassificadaComoBaixo()
    {
        double soma = 3 + 4 + 2 + 2 + 0; // 11
        int count = 5;

        var result = _service.CalcularIndicador("Semente Esverdeada", soma, count, _configsVazias);

        _output.WriteLine($"Semente Esverdeada → media={result.ValorMedio}, classificacao={result.Classificacao}, cor={result.Cor}");

        Assert.Equal(2.2, result.ValorMedio);
        Assert.Equal("Baixo", result.Classificacao);
        Assert.Equal("#90FF4C", result.Cor);
    }

    [Theory]
    [InlineData(0.5, 1, "Muito Baixo", "#317C53")]  // < 2
    [InlineData(2.0, 1, "Baixo",       "#90FF4C")]  // = 2 (min inclusivo)
    [InlineData(2.9, 1, "Baixo",       "#90FF4C")]  // [2, 3)
    [InlineData(3.0, 1, "Médio",       "#E1E86E")]  // = 3 (próximo intervalo)
    [InlineData(4.5, 1, "Médio",       "#E1E86E")]  // [3, 5)
    [InlineData(5.0, 1, "Alto",        "#EB883C")]  // = 5
    [InlineData(9.9, 1, "Alto",        "#EB883C")]  // [5, 10)
    [InlineData(10.0, 1, "Muito Alto", "#EB3F3F")]  // >= 10
    public void SementeEsverdeada_IntervalosEsperados(double soma, int count, string classificacaoEsperada, string corEsperada)
    {
        var result = _service.CalcularIndicador("Semente Esverdeada", soma, count, _configsVazias);

        _output.WriteLine($"Semente Esverdeada soma={soma} → media={result.ValorMedio}, classificacao={result.Classificacao}, cor={result.Cor}");

        Assert.Equal(classificacaoEsperada, result.Classificacao);
        Assert.Equal(corEsperada, result.Cor);
        Assert.NotNull(result.IntervaloAdequado);
    }

    // ─── CalcularIndicador – "Umidade" ─────────────────────────────────────────

    /// <summary>
    /// Média das 5 amostras = (14+16+11+20+22)/5 = 16.6 → intervalo [16,20) = "Alto", #EB883C
    /// </summary>
    [Fact]
    public void Umidade_MediaDasAmostras_DeveSerClassificadaComoAlto()
    {
        double soma = 14 + 16 + 11 + 20 + 22; // 83
        int count = 5;

        var result = _service.CalcularIndicador("Umidade", soma, count, _configsVazias);

        _output.WriteLine($"Umidade → media={result.ValorMedio}, classificacao={result.Classificacao}, cor={result.Cor}");

        Assert.Equal(16.6, result.ValorMedio);
        Assert.Equal("Alto", result.Classificacao);
        Assert.Equal("#EB883C", result.Cor);
    }

    [Theory]
    [InlineData(8.0,  1, "Muito Baixo", "#EB3F3F")]  // < 10
    [InlineData(10.0, 1, "Baixo",       "#90FF4C")]  // = 10
    [InlineData(12.0, 1, "Baixo",       "#90FF4C")]  // [10, 13)
    [InlineData(13.0, 1, "Adequado",    "#317C53")]  // = 13 (Adequado)
    [InlineData(15.0, 1, "Adequado",    "#317C53")]  // [13, 16)
    [InlineData(16.0, 1, "Alto",        "#EB883C")]  // = 16
    [InlineData(18.0, 1, "Alto",        "#EB883C")]  // [16, 20)
    [InlineData(20.0, 1, "Muito Alto",  "#EB3F3F")]  // >= 20
    public void Umidade_IntervalosEsperados(double soma, int count, string classificacaoEsperada, string corEsperada)
    {
        var result = _service.CalcularIndicador("Umidade", soma, count, _configsVazias);

        _output.WriteLine($"Umidade soma={soma} → media={result.ValorMedio}, classificacao={result.Classificacao}, cor={result.Cor}");

        Assert.Equal(classificacaoEsperada, result.Classificacao);
        Assert.Equal(corEsperada, result.Cor);
    }

    [Fact]
    public void Umidade_IntervaloAdequado_DeveSerEntre13e16()
    {
        var result = _service.CalcularIndicador("Umidade", 14, 1, _configsVazias); // Adequado

        _output.WriteLine($"Umidade IntervaloAdequado → min={result.IntervaloAdequado?.Min}, max={result.IntervaloAdequado?.Max}");

        Assert.NotNull(result.IntervaloAdequado);
        Assert.Equal(13.0, result.IntervaloAdequado!.Min);
        Assert.Equal(16.0, result.IntervaloAdequado.Max);
    }

    // ─── SemDados ────────────────────────────────────────────────────────────

    [Fact]
    public void CalcularIndicador_SemPontos_DeveRetornarSemDados()
    {
        var result = _service.CalcularIndicador("Semente Esverdeada", 0, 0, _configsVazias);

        _output.WriteLine($"Sem pontos → classificacao={result.Classificacao}");

        Assert.Equal("Sem dados", result.Classificacao);
    }

    // ─── BuscarEAcumularValor com trailing space ──────────────────────────────

    /// <summary>
    /// A coluna real do Excel tem nome "Semente Esverdeada " (trailing space).
    /// BuscarEAcumularValor deve acumulá-la via trim match.
    /// </summary>
    [Fact]
    public void BuscarEAcumularValor_ComTrailingSpace_DeveEncontrarValor()
    {
        var jsonData = System.Text.Json.JsonSerializer.Serialize(new[]
        {
            new Dictionary<string, object> { ["ID da Amostra"] = 1, ["Semente Esverdeada "] = 3.0 },
        });

        var elementos = JsonDocument.Parse(jsonData).RootElement.EnumerateArray().ToList();
        var ponto = elementos[0];

        double soma = 0;
        int count = 0;

        var chaves = new[] { "Semente Esverdeada", "SementeEsverdeada", "S.Esverdeada", "Esverdeada",
                             "Semente Envelhecida", "SementeEnvelhecida", "S.Envelhecida", "Envelhecida" };

        _service.BuscarEAcumularValor(ponto, chaves, ref soma, ref count);

        _output.WriteLine($"BuscarEAcumularValor (trailing space) → soma={soma}, count={count}");

        Assert.Equal(1, count);
        Assert.Equal(3.0, soma);
    }

    // ─── Integração: acumulação + classificação ───────────────────────────────

    [Fact]
    public void IntegraTeste_AcumularEClassificar_SementeEsverdeadaEUmidade()
    {
        var chavesSementeEsverdeada = new[] { "Semente Esverdeada", "SementeEsverdeada", "S.Esverdeada", "Esverdeada",
                                              "Semente Envelhecida", "SementeEnvelhecida", "S.Envelhecida", "Envelhecida" };
        var chavesUmidade = new[] { "Umidade", "Umid.", "UMIDADE" };

        var jsonData = JsonDocument.Parse(JsonRelatorio);

        double sumEsverdeada = 0, sumUmidade = 0;
        int countEsverdeada = 0, countUmidade = 0;

        foreach (var ponto in jsonData.RootElement.EnumerateArray())
        {
            _service.BuscarEAcumularValor(ponto, chavesSementeEsverdeada, ref sumEsverdeada, ref countEsverdeada);
            _service.BuscarEAcumularValor(ponto, chavesUmidade, ref sumUmidade, ref countUmidade);
        }

        var indicadorEsverdeada = _service.CalcularIndicador("Semente Esverdeada", sumEsverdeada, countEsverdeada, _configsVazias);
        var indicadorUmidade    = _service.CalcularIndicador("Umidade", sumUmidade, countUmidade, _configsVazias);

        _output.WriteLine($"Semente Esverdeada  → soma={sumEsverdeada}, count={countEsverdeada}, media={indicadorEsverdeada.ValorMedio}, classificacao={indicadorEsverdeada.Classificacao}, cor={indicadorEsverdeada.Cor}");
        _output.WriteLine($"Umidade             → soma={sumUmidade}, count={countUmidade}, media={indicadorUmidade.ValorMedio}, classificacao={indicadorUmidade.Classificacao}, cor={indicadorUmidade.Cor}");

        // Semente Esverdeada: soma=11, count=5, media=2.2 → "Baixo"
        Assert.Equal(5, countEsverdeada);
        Assert.Equal(2.2, indicadorEsverdeada.ValorMedio);
        Assert.Equal("Baixo", indicadorEsverdeada.Classificacao);
        Assert.NotEqual("#CCCCCC", indicadorEsverdeada.Cor);

        // Umidade: soma=83, count=5, media=16.6 → "Alto"
        Assert.Equal(5, countUmidade);
        Assert.Equal(16.6, indicadorUmidade.ValorMedio);
        Assert.Equal("Alto", indicadorUmidade.Classificacao);
        Assert.NotEqual("#CCCCCC", indicadorUmidade.Cor);
    }
}
