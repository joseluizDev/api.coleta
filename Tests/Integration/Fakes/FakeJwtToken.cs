using api.cliente.Interfaces;
using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;

namespace api.coleta.Tests.Integration.Fakes;

public class FakeJwtToken : IJwtToken
{
    private readonly Func<Guid> _userResolver;

    public FakeJwtToken(Func<Guid> userResolver)
    {
        _userResolver = userResolver;
    }

    public Guid? LastUserId { get; private set; }

    public string GerarToken(Usuario usuario) => "fake-access-token";
    public bool ValidarToken(string token) => true;
    public Guid? ObterUsuarioIdDoToken(string token)
    {
        LastUserId = _userResolver();
        return LastUserId;
    }

    public string GerarRefreshToken(Usuario usuario) => "fake-refresh-token";
    public bool ValidarRefreshToken(string refreshToken) => true;
    public Guid? ObterUsuarioIdDoRefreshToken(string refreshToken)
    {
        LastUserId = _userResolver();
        return LastUserId;
    }

    public TokenPairDTO GerarParDeTokens(Usuario usuario) => new()
    {
        AccessToken = "fake-access-token",
        RefreshToken = "fake-refresh-token",
        ExpiresIn = 1200
    };
}
