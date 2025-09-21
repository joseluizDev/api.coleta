using System.Net;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using api.coleta.Data;
using api.coleta.Models.DTOs;
using api.coleta.Tests.Helpers;
using api.coleta.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace api.coleta.Tests.Integration;

public class RelatorioControllerIntegrationTests : IAsyncLifetime
{
    private readonly TestApplicationFactory _factory;
    private readonly HttpClient _client;

    public RelatorioControllerIntegrationTests()
    {
        _factory = new TestApplicationFactory();
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task GetRelatorio_DeveRetornarDadosCompleto()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (usuarioId, coleta, relatorio) = RelatorioTestData.SeedRelatorios(context);
        Assert.Equal(coleta.Id, relatorio.ColetaId);
        var existentes = context.Relatorios.ToList();
        Assert.Contains(existentes, r => r.UsuarioId == usuarioId && r.ColetaId == coleta.Id && !string.IsNullOrEmpty(r.LinkBackup));
        var serviceDirect = scope.ServiceProvider.GetRequiredService<RelatorioService>();
        var dtoDireto = await serviceDirect.GetRelario(coleta.Id, usuarioId);
        Assert.NotNull(dtoDireto);
        _factory.TestUserId = usuarioId;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "fake-token");

        var response = await _client.GetAsync($"/api/relatorio/{coleta.Id}");
        var body = await response.Content.ReadAsStringAsync();
        var resolvedId = _factory.JwtToken.LastUserId;

        Assert.Equal(usuarioId, resolvedId ?? Guid.Empty);
        Assert.True(response.IsSuccessStatusCode, body);
        var dto = await response.Content.ReadFromJsonAsync<RelatorioOuputDTO>();

        Assert.NotNull(dto);
        Assert.Equal(relatorio.Id, dto!.Id);
        Assert.Equal(coleta.NomeColeta, dto.NomeColeta);
        Assert.Equal(relatorio.LinkBackup, dto.LinkBackup);
        Assert.Contains("Macronutrientes", dto.TiposAnalise);
        Assert.Equal(relatorio.JsonRelatorio, dto.JsonRelatorio);
        Assert.True(dto.IsRelatorio);
    }

    [Fact]
    public async Task GetRelatorio_ComIdInvalido_DeveRetornarRespostaSemRelatorio()
    {
        _factory.TestUserId = Guid.NewGuid();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "fake-token");

        var response = await _client.GetAsync($"/api/relatorio/{Guid.NewGuid()}");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(body), $"body: {body}");
        var dto = await response.Content.ReadFromJsonAsync<RelatorioOuputDTO>();

        Assert.NotNull(dto);
        Assert.False(dto!.IsRelatorio);
        Assert.Null(dto.JsonRelatorio);
    }

    [Fact]
    public async Task AtualizarJsonRelatorio_DeveSobrescreverConteudo()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (usuarioId, coleta, relatorio) = RelatorioTestData.SeedRelatorios(context);
        _factory.TestUserId = usuarioId;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "fake-token");

        var payload = new AtualizarJsonRelatorioDTO
        {
            ColetaId = coleta.Id,
            JsonRelatorio = "{\"value\":42}"
        };

        var response = await _client.PutAsJsonAsync("/api/relatorio/atualizar/JsonRelatorio", payload);
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(response.IsSuccessStatusCode, body);

        using var assertScope = _factory.Services.CreateScope();
        var db = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var atualizado = db.Relatorios.Single(r => r.Id == relatorio.Id);

        Assert.Equal(payload.JsonRelatorio, atualizado.JsonRelatorio);
    }

    [Fact]
    public async Task AtualizarJsonRelatorio_QuandoNaoEncontrar_DeveRetornarNotFound()
    {
        _factory.TestUserId = Guid.NewGuid();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "fake-token");

        var payload = new AtualizarJsonRelatorioDTO
        {
            ColetaId = Guid.NewGuid(),
            JsonRelatorio = "{}"
        };

        var response = await _client.PutAsJsonAsync("/api/relatorio/atualizar/JsonRelatorio", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ListarRelatoriosPorUpload_DeveRetornarLista()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (usuarioId, _, _) = RelatorioTestData.SeedRelatorios(context);
        _factory.TestUserId = usuarioId;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "fake-token");

        var response = await _client.GetAsync("/api/relatorio/buscar");
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(response.IsSuccessStatusCode, body);
        var dtos = await response.Content.ReadFromJsonAsync<List<RelatorioOuputDTO>>();

        Assert.NotNull(dtos);
        Assert.Single(dtos!);
        Assert.Equal("Coleta Principal", dtos[0].NomeColeta);
    }

    [Fact]
    public async Task UploadData_DevePersistirRelatorioENotificarSucesso()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (usuarioId, coleta, _) = RelatorioTestData.SeedRelatorios(context);
        _factory.TestUserId = usuarioId;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "fake-token");

        using var content = new MultipartFormDataContent();
        var arquivo = new ByteArrayContent(Encoding.UTF8.GetBytes("conteudo"));
        arquivo.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(arquivo, "Arquivo", "relatorio.pdf");
        content.Add(new StringContent("{\"value\":1}"), "ArquivoJson");
        content.Add(new StringContent(coleta.Id.ToString()), "ColetaId");

        var response = await _client.PostAsync("/api/relatorio/upload", content);
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(response.IsSuccessStatusCode, body);

        using var assertScope = _factory.Services.CreateScope();
        var db = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var relatorios = db.Relatorios.Where(r => r.UsuarioId == usuarioId).ToList();

        Assert.True(relatorios.Count >= 2);
        Assert.Contains(relatorios, r => !string.IsNullOrEmpty(r.LinkBackup));
        Assert.NotEmpty(_factory.MinioStorage.Uploads);
    }
}
