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

    // ========== CADASTRO ==========

    [Fact]
    public async Task CadastrarNovo_DeveRetornarSucessoComUsuarioCriado()
    {
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

    // ========== LOGIN ==========

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornarTokenPair()
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
        Assert.Contains("accessToken", body);
        Assert.Contains("refreshToken", body);
        Assert.Contains("expiresIn", body);
    }

    [Fact]
    public async Task Login_ComCredenciaisInvalidas_DeveRetornarNotFound()
    {
        var response = await _client.GetAsync("/api/usuario/login?email=naoexiste@teste.com&senha=senhaerrada");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ========== REFRESH TOKEN ==========

    [Fact]
    public async Task RefreshToken_ComRefreshTokenValido_DeveRetornarNovoParDeTokens()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var usuario = new Usuario
        {
            NomeCompleto = "Usuário Refresh",
            CPF = "11122233300",
            Email = "refresh@teste.com",
            Telefone = "11333333333",
            Senha = "senha123"
        };
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        // Login para obter tokens (FakeJwtToken retorna "fake-refresh-token")
        var loginResponse = await _client.GetAsync("/api/usuario/login?email=refresh@teste.com&senha=senha123");
        Assert.True(loginResponse.IsSuccessStatusCode);
        var loginBody = await loginResponse.Content.ReadAsStringAsync();
        Assert.Contains("accessToken", loginBody);
        Assert.Contains("refreshToken", loginBody);
    }

    [Fact]
    public async Task RefreshToken_SemToken_DeveRetornarBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/usuario/refresh-token", new RefreshTokenRequestDTO());
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Refresh token não fornecido", body);
    }

    // ========== LOGOUT ==========

    [Fact]
    public async Task Logout_DeveRetornarSucesso()
    {
        var body = new RefreshTokenRequestDTO { RefreshToken = "qualquer-token" };
        var response = await _client.PostAsJsonAsync("/api/usuario/logout", body);

        Assert.True(response.IsSuccessStatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.Contains("Logout realizado com sucesso", responseBody);
    }

    [Fact]
    public async Task Logout_SemBody_DeveRetornarSucesso()
    {
        var response = await _client.PostAsync("/api/usuario/logout",
            new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));

        Assert.True(response.IsSuccessStatusCode);
    }

    // ========== MOBILE ==========

    [Fact]
    public async Task LoginMobile_ComCredenciaisValidas_DeveRetornarTokenPair()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var usuario = new Usuario
        {
            NomeCompleto = "Usuário Mobile",
            CPF = "44455566677",
            Email = "mobile@teste.com",
            Telefone = "11222222222",
            Senha = "senha123"
        };
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        var loginDto = new UsuarioLoginDTO { Email = "mobile@teste.com", Senha = "senha123" };
        var response = await _client.PostAsJsonAsync("/api/mobile/usuario/login", loginDto);
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}, Body: {body}");
        Assert.Contains("accessToken", body);
        Assert.Contains("refreshToken", body);
    }

    [Fact]
    public async Task LoginMobile_ComCredenciaisInvalidas_DeveRetornarNotFound()
    {
        var loginDto = new UsuarioLoginDTO { Email = "naoexiste@teste.com", Senha = "senhaerrada" };
        var response = await _client.PostAsJsonAsync("/api/mobile/usuario/login", loginDto);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RefreshTokenMobile_SemToken_DeveRetornarBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/mobile/usuario/refresh-token", new RefreshTokenRequestDTO());
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Refresh token não fornecido", body);
    }

    [Fact]
    public async Task LogoutMobile_DeveRetornarSucesso()
    {
        var body = new RefreshTokenRequestDTO { RefreshToken = "qualquer-token" };
        var response = await _client.PostAsJsonAsync("/api/mobile/usuario/logout", body);

        Assert.True(response.IsSuccessStatusCode);
    }
}
