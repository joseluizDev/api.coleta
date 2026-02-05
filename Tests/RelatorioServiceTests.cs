using api.coleta.Models.DTOs;
using api.coleta.Repositories;
using api.coleta.Services;
using api.coleta.Services.Relatorio;
using api.coleta.Tests.Fakes;
using api.coleta.Tests.Helpers;
using api.coleta.Data;
using api.coleta.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace api.coleta.Tests;

public class RelatorioServiceTests
{
    private static RelatorioService CreateService(ApplicationDbContext context)
    {
        var repository = new RelatorioRepository(context);
        var minioStorage = new FakeMinioStorage();
        var geoJsonRepository = new GeoJsonRepository(context);
        var nutrientConfigRepository = new NutrientConfigRepository(context);
        var recomendacaoRepository = new RecomendacaoRepository(context);
        var unitOfWork = new UnitOfWorkImplements(context);

        // Criar os novos services extraídos
        var classificationService = new NutrientClassificationService();
        var geoJsonProcessorService = new GeoJsonProcessorService(geoJsonRepository);
        var statisticsService = new AttributeStatisticsService(classificationService);
        var indicatorService = new SoilIndicatorService(classificationService);

        return new RelatorioService(
            repository,
            minioStorage,
            nutrientConfigRepository,
            classificationService,
            geoJsonProcessorService,
            statisticsService,
            indicatorService,
            recomendacaoRepository,
            unitOfWork);
    }

    [Fact]
    public async Task GetRelario_DeveRetornarDtoQuandoRelatorioExiste()
    {
        using var context = TestHelper.CreateInMemoryContext();
        var service = CreateService(context);

        var (usuarioId, coleta, relatorio) = RelatorioTestData.SeedRelatorios(context);

        var resultado = await service.GetRelario(coleta.Id, usuarioId);

        Assert.NotNull(resultado);
        Assert.Equal(relatorio.Id, resultado!.Id);
        Assert.Equal(coleta.NomeColeta, resultado.NomeColeta);
        Assert.Equal(ProfundidadeFormatter.Formatar(coleta.Profundidade), resultado.Profundidade);
        Assert.Equal(relatorio.LinkBackup, resultado.LinkBackup);
        Assert.Contains("Macronutrientes", resultado.TiposAnalise);
        Assert.Equal(relatorio.JsonRelatorio, resultado.JsonRelatorio);
        Assert.True(resultado.IsRelatorio);
    }

    [Fact]
    public async Task GetRelario_DeveRetornarNullQuandoNaoEncontrado()
    {
        using var context = TestHelper.CreateInMemoryContext();
        var service = CreateService(context);

        var resultado = await service.GetRelario(Guid.NewGuid(), Guid.NewGuid());

        Assert.Null(resultado);
    }

    [Fact]
    public async Task ListarRelatoriosPorUploadAsync_DeveRetornarListaVaziaQuandoNaoExistirRegistros()
    {
        using var context = TestHelper.CreateInMemoryContext();
        var service = CreateService(context);

        var resultado = await service.ListarRelatoriosPorUploadAsync(Guid.NewGuid(), new api.coleta.models.dtos.QueryRelatorio());

        Assert.Empty(resultado.Items);
    }

    [Fact]
    public async Task ListarRelatoriosPorUploadAsync_DeveMapearRelatoriosParaDto()
    {
        using var context = TestHelper.CreateInMemoryContext();
        var service = CreateService(context);

        var (usuarioId, coleta, _) = RelatorioTestData.SeedRelatorios(context);

        var resultado = await service.ListarRelatoriosPorUploadAsync(usuarioId, new api.coleta.models.dtos.QueryRelatorio());

        Assert.Single(resultado.Items);
        var dto = resultado.Items.First();
        Assert.Equal(coleta.NomeColeta, dto.NomeColeta);
        Assert.Equal(coleta.Talhao.Nome, dto.Talhao);
        Assert.NotNull(dto.Safra);
        Assert.Equal(coleta.Safra!.Observacao, dto.Safra.Observacao);
        Assert.Equal(coleta.Safra!.DataInicio, dto.Safra.DataInicio);
        Assert.Equal(coleta.Safra!.DataFim, dto.Safra.DataFim);
        Assert.Equal(coleta.UsuarioResp!.NomeCompleto, dto.Funcionario);
    }

    [Fact]
    public async Task AtualizarJsonRelatorioAsync_DeveSobrescreverConteudoQuandoExistir()
    {
        using var context = TestHelper.CreateInMemoryContext();
        var service = CreateService(context);

        var (usuarioId, coleta, relatorio) = RelatorioTestData.SeedRelatorios(context);
        var novoJson = "{\"novo\":true}";

        var atualizado = await service.AtualizarJsonRelatorioAsync(coleta.Id, relatorio.Id, usuarioId, novoJson);

        Assert.True(atualizado);
        var entidade = context.Relatorios.Single(r => r.Id == relatorio.Id);
        Assert.Equal(novoJson, entidade.JsonRelatorio);
    }

            [Fact]
    public async Task AtualizarJsonRelatorioAsync_DeveRetornarFalseQuandoNaoEncontrarRelatorio()
    {
        using var context = TestHelper.CreateInMemoryContext();
        var service = CreateService(context);
    
        var atualizado = await service.AtualizarJsonRelatorioAsync(Guid.NewGuid(), Guid.NewGuid(),Guid.NewGuid(), "{}");

        Assert.False(atualizado);
    }
}
