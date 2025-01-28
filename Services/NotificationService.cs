using Minio.Exceptions;

public class Notificador : INotificador
{
   private List<Notificacao> _notificacoes;

   public Notificador()
   {
      _notificacoes = new List<Notificacao>();
   }

   public void Notificar(Notificacao notificacao)
   {
      _notificacoes.Add(notificacao);
   }

   public List<Notificacao> ObterNotificacoes()
   {
      return _notificacoes;
   }

   public bool TemNotificacao()
   {
      return _notificacoes.Any();
   }
}

public class Notificacao
{
   public string Mensagem { get; set; }

   public Notificacao(string mensagem)
   {
      Mensagem = mensagem;
   }
}
