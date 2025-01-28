public interface INotificador
{
   bool TemNotificacao();
   List<Notificacao> ObterNotificacoes();
   void Notificar(Notificacao notificacao);
}