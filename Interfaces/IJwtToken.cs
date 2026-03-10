using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;

namespace api.cliente.Interfaces
{
    public interface IJwtToken
    {
        // Access token
        string GerarToken(Usuario usuario);
        bool ValidarToken(string token);
        Guid? ObterUsuarioIdDoToken(string token);

        // Refresh token
        string GerarRefreshToken(Usuario usuario);
        bool ValidarRefreshToken(string refreshToken);
        Guid? ObterUsuarioIdDoRefreshToken(string refreshToken);

        // Par de tokens (login)
        TokenPairDTO GerarParDeTokens(Usuario usuario);
    }
}
