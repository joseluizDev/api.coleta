using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using api.coleta.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;

namespace api.coleta.Tests;

public class RelatorioRepositoryTests
{
    [Fact]
    public async Task ListarRelatoriosPorUploadAsync_DeveRetornarSomenteRelatoriosComLinkDoUsuario()
    {
        using var context = TestHelper.CreateInMemoryContext();
        var repository = new RelatorioRepository(context);

        var (usuarioId, _, _) = RelatorioTestData.SeedRelatorios(context);

        var resultado = await repository.ListarRelatoriosPorUploadAsync(usuarioId, new api.coleta.models.dtos.QueryRelatorio());

        Assert.Single(resultado.Items);
        Assert.All(resultado.Items, item =>
        {
            Assert.Equal(usuarioId, item.UsuarioId);
            Assert.False(string.IsNullOrEmpty(item.LinkBackup));
            Assert.NotNull(item.Coleta);
            Assert.Equal("Coleta Principal", item.Coleta?.NomeColeta);
            Assert.Equal("Talhao 1", item.Coleta?.Talhao?.Nome);
        });
    }

    [Fact]
    public async Task ObterPorId_DeveTrazerRelatorioComRelacionamentos()
    {
        using var context = TestHelper.CreateInMemoryContext();
        var repository = new RelatorioRepository(context);

        var (usuarioId, coleta, _) = RelatorioTestData.SeedRelatorios(context);

        var resultado = await repository.ObterPorId(coleta.Id, usuarioId);

        Assert.NotNull(resultado);
        Assert.Equal(coleta.Id, resultado!.ColetaId);
        Assert.NotNull(resultado.Coleta);
        Assert.NotNull(resultado.Coleta!.Talhao);
        Assert.NotNull(resultado.Coleta!.Safra);
        Assert.NotNull(resultado.Coleta!.UsuarioResp);
    }
}
