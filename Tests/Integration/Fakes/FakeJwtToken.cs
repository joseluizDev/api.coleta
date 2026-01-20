using api.cliente.Interfaces;
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

    public string GerarToken(Usuario usuario) => "fake-token";

    public bool ValidarToken(string token) => true;

    public Guid? ObterUsuarioIdDoToken(string token)
    {
        LastUserId = _userResolver();
        return LastUserId;
    }
}
