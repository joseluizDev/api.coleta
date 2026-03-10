using api.coleta.Services;
using Xunit;

namespace api.coleta.Tests;

public class RefreshTokenStoreTests : IDisposable
{
    private readonly RefreshTokenStore _store;

    public RefreshTokenStoreTests()
    {
        _store = new RefreshTokenStore();
    }

    public void Dispose()
    {
        _store.Dispose();
    }

    [Fact]
    public void Adicionar_DevePermitirValidarToken()
    {
        var token = "refresh-token-123";
        var userId = Guid.NewGuid();
        var expira = DateTime.UtcNow.AddDays(7);

        _store.Adicionar(token, userId, expira);

        Assert.True(_store.Validar(token));
    }

    [Fact]
    public void Validar_TokenInexistente_DeveRetornarFalse()
    {
        Assert.False(_store.Validar("token-que-nao-existe"));
    }

    [Fact]
    public void Validar_TokenExpirado_DeveRetornarFalse()
    {
        var token = "refresh-token-expirado";
        var userId = Guid.NewGuid();
        var expirado = DateTime.UtcNow.AddMinutes(-1);

        _store.Adicionar(token, userId, expirado);

        Assert.False(_store.Validar(token));
    }

    [Fact]
    public void Revogar_DeveInvalidarToken()
    {
        var token = "refresh-token-revogado";
        var userId = Guid.NewGuid();

        _store.Adicionar(token, userId, DateTime.UtcNow.AddDays(7));
        Assert.True(_store.Validar(token));

        _store.Revogar(token);
        Assert.False(_store.Validar(token));
    }

    [Fact]
    public void RevogarTodosDoUsuario_DeveRemoverApenasTokensDoUsuario()
    {
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var expira = DateTime.UtcNow.AddDays(7);

        _store.Adicionar("token-user1-a", userId1, expira);
        _store.Adicionar("token-user1-b", userId1, expira);
        _store.Adicionar("token-user2-a", userId2, expira);

        _store.RevogarTodosDoUsuario(userId1);

        Assert.False(_store.Validar("token-user1-a"));
        Assert.False(_store.Validar("token-user1-b"));
        Assert.True(_store.Validar("token-user2-a"));
    }

    [Fact]
    public void ContarAtivos_DeveRetornarApenasNaoExpirados()
    {
        var userId = Guid.NewGuid();

        _store.Adicionar("token-ativo-1", userId, DateTime.UtcNow.AddDays(7));
        _store.Adicionar("token-ativo-2", userId, DateTime.UtcNow.AddDays(7));
        _store.Adicionar("token-expirado", userId, DateTime.UtcNow.AddMinutes(-1));

        Assert.Equal(2, _store.ContarAtivos());
    }

    [Fact]
    public void Revogar_TokenInexistente_NaoDeveLancarExcecao()
    {
        var exception = Record.Exception(() => _store.Revogar("token-inexistente"));
        Assert.Null(exception);
    }

    [Fact]
    public void Adicionar_MesmoToken_DeveSubstituir()
    {
        var token = "token-duplicado";
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        _store.Adicionar(token, userId1, DateTime.UtcNow.AddDays(7));
        _store.Adicionar(token, userId2, DateTime.UtcNow.AddDays(7));

        // Deve continuar válido (sobrescreveu)
        Assert.True(_store.Validar(token));
    }
}
