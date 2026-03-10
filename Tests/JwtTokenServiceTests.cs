using api.coleta.Models.Entidades;
using api.coleta.Services;
using BackAppPromo.Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace api.coleta.Tests;

public class JwtTokenServiceTests : IDisposable
{
    private readonly JwtTokenService _service;
    private readonly RefreshTokenStore _store;
    private readonly Usuario _usuario;

    private const string SecretKey = "chave_secreta_access_token_teste_12345678901234567890";
    private const string RefreshSecretKey = "chave_secreta_refresh_token_teste_12345678901234567890";
    private const string Issuer = "test_issuer";
    private const string Audience = "test_audience";

    public JwtTokenServiceTests()
    {
        _store = new RefreshTokenStore();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = SecretKey,
                ["Jwt:RefreshSecretKey"] = RefreshSecretKey,
                ["Jwt:Issuer"] = Issuer,
                ["Jwt:Audience"] = Audience,
            })
            .Build();

        _service = new JwtTokenService(config, _store);

        _usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            NomeCompleto = "Teste Usuario",
            Email = "teste@teste.com",
            CPF = "12345678901",
            Telefone = "11999999999",
            Senha = "senha123"
        };
    }

    public void Dispose()
    {
        _store.Dispose();
    }

    // ========== GERAÇÃO DE TOKEN ==========

    [Fact]
    public void GerarToken_DeveRetornarStringNaoVazia()
    {
        var token = _service.GerarToken(_usuario);

        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void GerarRefreshToken_DeveRetornarStringNaoVazia()
    {
        var refreshToken = _service.GerarRefreshToken(_usuario);

        Assert.NotNull(refreshToken);
        Assert.NotEmpty(refreshToken);
    }

    [Fact]
    public void GerarParDeTokens_DeveRetornarAmbosTokens()
    {
        var par = _service.GerarParDeTokens(_usuario);

        Assert.NotNull(par);
        Assert.NotEmpty(par.AccessToken);
        Assert.NotEmpty(par.RefreshToken);
        Assert.True(par.ExpiresIn > 0);
    }

    [Fact]
    public void GerarParDeTokens_AccessERefreshDevemSerDiferentes()
    {
        var par = _service.GerarParDeTokens(_usuario);

        Assert.NotEqual(par.AccessToken, par.RefreshToken);
    }

    // ========== VALIDAÇÃO DE ACCESS TOKEN ==========

    [Fact]
    public void ValidarToken_AccessTokenValido_DeveRetornarTrue()
    {
        var token = _service.GerarToken(_usuario);

        Assert.True(_service.ValidarToken(token));
    }

    [Fact]
    public void ValidarToken_RefreshTokenUsadoComoAccess_DeveRetornarFalse()
    {
        var refreshToken = _service.GerarRefreshToken(_usuario);

        // Refresh token NÃO deve ser aceito como access token
        Assert.False(_service.ValidarToken(refreshToken));
    }

    [Fact]
    public void ValidarToken_TokenInvalido_DeveRetornarFalse()
    {
        Assert.False(_service.ValidarToken("token-invalido-lixo"));
    }

    [Fact]
    public void ValidarToken_TokenVazio_DeveRetornarFalse()
    {
        Assert.False(_service.ValidarToken(""));
    }

    // ========== VALIDAÇÃO DE REFRESH TOKEN ==========

    [Fact]
    public void ValidarRefreshToken_RefreshTokenValido_DeveRetornarTrue()
    {
        var refreshToken = _service.GerarRefreshToken(_usuario);

        Assert.True(_service.ValidarRefreshToken(refreshToken));
    }

    [Fact]
    public void ValidarRefreshToken_AccessTokenUsadoComoRefresh_DeveRetornarFalse()
    {
        var accessToken = _service.GerarToken(_usuario);

        // Access token NÃO deve ser aceito como refresh token
        Assert.False(_service.ValidarRefreshToken(accessToken));
    }

    [Fact]
    public void ValidarRefreshToken_TokenInvalido_DeveRetornarFalse()
    {
        Assert.False(_service.ValidarRefreshToken("lixo-total"));
    }

    [Fact]
    public void ValidarRefreshToken_RevogadoNoStore_DeveRetornarFalse()
    {
        var refreshToken = _service.GerarRefreshToken(_usuario);
        Assert.True(_service.ValidarRefreshToken(refreshToken));

        // Revogar do store
        _store.Revogar(refreshToken);

        // JWT ainda é criptograficamente válido, mas store rejeita
        Assert.False(_service.ValidarRefreshToken(refreshToken));
    }

    // ========== EXTRAÇÃO DE USER ID ==========

    [Fact]
    public void ObterUsuarioIdDoToken_DeveRetornarIdCorreto()
    {
        var token = _service.GerarToken(_usuario);

        var userId = _service.ObterUsuarioIdDoToken(token);

        Assert.NotNull(userId);
        Assert.Equal(_usuario.Id, userId.Value);
    }

    [Fact]
    public void ObterUsuarioIdDoRefreshToken_DeveRetornarIdCorreto()
    {
        var refreshToken = _service.GerarRefreshToken(_usuario);

        var userId = _service.ObterUsuarioIdDoRefreshToken(refreshToken);

        Assert.NotNull(userId);
        Assert.Equal(_usuario.Id, userId.Value);
    }

    [Fact]
    public void ObterUsuarioIdDoRefreshToken_TokenRevogado_DeveRetornarNull()
    {
        var refreshToken = _service.GerarRefreshToken(_usuario);
        _store.Revogar(refreshToken);

        var userId = _service.ObterUsuarioIdDoRefreshToken(refreshToken);

        Assert.Null(userId);
    }

    [Fact]
    public void ObterUsuarioIdDoToken_TokenInvalido_DeveRetornarNull()
    {
        var userId = _service.ObterUsuarioIdDoToken("token-invalido");

        Assert.Null(userId);
    }

    [Fact]
    public void ObterUsuarioIdDoToken_TokenNuloOuVazio_DeveRetornarNull()
    {
        Assert.Null(_service.ObterUsuarioIdDoToken(""));
        Assert.Null(_service.ObterUsuarioIdDoToken(null!));
    }

    // ========== ISOLAMENTO CRIPTOGRÁFICO ==========

    [Fact]
    public void TokensDevemTerChavesDiferentes_AccessNaoValidaComoRefresh()
    {
        var par = _service.GerarParDeTokens(_usuario);

        // Access token validado com chave de access: OK
        Assert.True(_service.ValidarToken(par.AccessToken));
        // Access token validado com chave de refresh: FALHA
        Assert.False(_service.ValidarRefreshToken(par.AccessToken));

        // Refresh token validado com chave de refresh: OK
        Assert.True(_service.ValidarRefreshToken(par.RefreshToken));
        // Refresh token validado com chave de access: FALHA
        Assert.False(_service.ValidarToken(par.RefreshToken));
    }

    // ========== ROTAÇÃO DE TOKENS ==========

    [Fact]
    public void RotacaoDeTokens_NovoRefreshDeveInvalidarAntigo()
    {
        // Simula login
        var par1 = _service.GerarParDeTokens(_usuario);
        Assert.True(_service.ValidarRefreshToken(par1.RefreshToken));

        // Simula refresh: revoga o antigo
        _store.Revogar(par1.RefreshToken);
        var par2 = _service.GerarParDeTokens(_usuario);

        // Token antigo: inválido
        Assert.False(_service.ValidarRefreshToken(par1.RefreshToken));
        // Token novo: válido
        Assert.True(_service.ValidarRefreshToken(par2.RefreshToken));
    }

    // ========== REGISTRO NO STORE ==========

    [Fact]
    public void GerarRefreshToken_DeveRegistrarNoStore()
    {
        var countAntes = _store.ContarAtivos();

        _service.GerarRefreshToken(_usuario);

        Assert.Equal(countAntes + 1, _store.ContarAtivos());
    }

    [Fact]
    public void GerarParDeTokens_DeveRegistrarRefreshNoStore()
    {
        var countAntes = _store.ContarAtivos();

        _service.GerarParDeTokens(_usuario);

        Assert.Equal(countAntes + 1, _store.ContarAtivos());
    }

    // ========== MÚLTIPLOS USUÁRIOS ==========

    [Fact]
    public void UsuariosDiferentes_DevemTerTokensDiferentes()
    {
        var usuario2 = new Usuario
        {
            Id = Guid.NewGuid(),
            NomeCompleto = "Outro Usuario",
            Email = "outro@teste.com",
            CPF = "98765432109",
            Telefone = "11888888888",
            Senha = "senha456"
        };

        var par1 = _service.GerarParDeTokens(_usuario);
        var par2 = _service.GerarParDeTokens(usuario2);

        Assert.NotEqual(par1.AccessToken, par2.AccessToken);
        Assert.NotEqual(par1.RefreshToken, par2.RefreshToken);

        // Cada token deve extrair o userId correto
        Assert.Equal(_usuario.Id, _service.ObterUsuarioIdDoToken(par1.AccessToken));
        Assert.Equal(usuario2.Id, _service.ObterUsuarioIdDoToken(par2.AccessToken));
    }
}
