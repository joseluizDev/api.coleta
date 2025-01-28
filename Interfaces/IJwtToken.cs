using api.coleta.Models.Entidades;

namespace api.cliente.Interfaces
{
    public interface IJwtToken
    {
        string GerarToken(Usuario usuario);
        bool ValidarToken(string token);
        Guid? ObterUsuarioIdDoToken(string token);
    }
}
