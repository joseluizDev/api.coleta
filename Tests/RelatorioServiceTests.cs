using api.coleta.Models.DTOs;
using api.coleta.Repositories;
using api.coleta.Services;
using api.coleta.Tests.Fakes;
using api.coleta.Tests.Helpers;
using api.coleta.Data;
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
        var unitOfWork = new UnitOfWorkImplements(context);
        return new RelatorioService(repository, minioStorage, unitOfWork);
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
        Assert.Equal(coleta.Profundidade.ToString(), resultado.Profundidade);
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

        var resultado = await service.ListarRelatoriosPorUploadAsync(Guid.NewGuid());

        Assert.Empty(resultado);
    }

    [Fact]
    public async Task ListarRelatoriosPorUploadAsync_DeveMapearRelatoriosParaDto()
    {
        using var context = TestHelper.CreateInMemoryContext();
        var service = CreateService(context);

        var (usuarioId, coleta, _) = RelatorioTestData.SeedRelatorios(context);

        var resultado = await service.ListarRelatoriosPorUploadAsync(usuarioId);

        Assert.Single(resultado);
        var dto = resultado.First();
        Assert.Equal(coleta.NomeColeta, dto.NomeColeta);
        Assert.Equal(coleta.Talhao.Nome, dto.Talhao);
        Assert.Equal(coleta.Safra!.Observacao, dto.Safra);
        Assert.Equal(coleta.UsuarioResp!.NomeCompleto, dto.Funcionario);
    }
}
