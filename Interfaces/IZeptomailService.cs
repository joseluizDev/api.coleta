using api.coleta.Models.DTOs;

namespace api.coleta.Interfaces
{
    public interface IZeptomailService
    {
        Task<bool> EnviarEmailConfirmacaoAsync(string nomeUsuario, string emailUsuario);
        Task<bool> EnviarEmailNotificacaoAdminsAsync(ContatoRequestDTO contato);
    }
}
