namespace api.coleta.Interfaces
{
    public interface IOneSignalService
    {
        Task<bool> EnviarNotificacaoAsync(string fcmToken, string titulo, string mensagem);
    }
}
