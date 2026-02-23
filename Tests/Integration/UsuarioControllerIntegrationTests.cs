using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using api.coleta.Data;
using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace api.coleta.Tests.Integration;

public class UsuarioControllerIntegrationTests : IAsyncLifetime
{
    private readonly TestApplicationFactory _factory;
    private readonly HttpClient _client;

    public UsuarioControllerIntegrationTests()
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
    public async Task CadastrarNovo_DeveRetornarSucessoComUsuarioCriado()
    {
        // CPF válido: 529.982.247-25 (sem pontuação: 52998224725)
        var novoUsuario = new UsuarioResquestDTO
        {
            NomeCompleto = "João da Silva",
            CPF = "52998224725",
            Email = "joao@teste.com",
            Telefone = "11999999999",
            Senha = "senha123"
        };

        var response = await _client.PostAsJsonAsync("/api/usuario/cadastrar", novoUsuario);
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}, Body: {body}");

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var usuarioCriado = context.Usuarios.FirstOrDefault(u => u.Email == "joao@teste.com");

        Assert.NotNull(usuarioCriado);
        Assert.Equal("João da Silva", usuarioCriado!.NomeCompleto);
        Assert.Equal("52998224725", usuarioCriado.CPF);
    }

    [Fact]
    public async Task CadastrarNovo_ComEmailDuplicado_DeveRetornarConflito()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var usuarioExistente = new Usuario
        {
            NomeCompleto = "Usuário Existente",
            CPF = "98765432100",
            Email = "existente@teste.com",
            Telefone = "11888888888",
            Senha = "senha123"
        };
        context.Usuarios.Add(usuarioExistente);
        await context.SaveChangesAsync();

        var novoUsuario = new UsuarioResquestDTO
        {
            NomeCompleto = "Outro Usuário",
            CPF = "11122233344",
            Email = "existente@teste.com",
            Telefone = "11777777777",
            Senha = "senha456"
        };

        var response = await _client.PostAsJsonAsync("/api/usuario/cadastrar", novoUsuario);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Email já cadastrado", body);
    }

    [Fact]
    public async Task CadastrarNovo_ComCpfDuplicado_DeveRetornarConflito()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var usuarioExistente = new Usuario
        {
            NomeCompleto = "Usuário Existente",
            CPF = "55544433322",
            Email = "outro@teste.com",
            Telefone = "11666666666",
            Senha = "senha123"
        };
        context.Usuarios.Add(usuarioExistente);
        await context.SaveChangesAsync();

        var novoUsuario = new UsuarioResquestDTO
        {
            NomeCompleto = "Novo Usuário",
            CPF = "55544433322",
            Email = "novo@teste.com",
            Telefone = "11555555555",
            Senha = "senha789"
        };

        var response = await _client.PostAsJsonAsync("/api/usuario/cadastrar", novoUsuario);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("CPF já cadastrado", body);
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornarToken()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var usuario = new Usuario
        {
            NomeCompleto = "Usuário Login",
            CPF = "99988877766",
            Email = "login@teste.com",
            Telefone = "11444444444",
            Senha = "senha123"
        };
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/usuario/login?email=login@teste.com&senha=senha123");
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}, Body: {body}");
        Assert.Contains("token", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_ComCredenciaisInvalidas_DeveRetornarNotFound()
    {
        var response = await _client.GetAsync("/api/usuario/login?email=naoexiste@teste.com&senha=senhaerrada");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
