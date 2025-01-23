using Microsoft.AspNetCore.Mvc;

namespace api.coleta.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected readonly INotificador Notificador;
        protected BaseController(INotificador notificador)
        {
            Notificador = notificador;
        }
        public IActionResult CustonResponse(object result = null)
        {
            if (Notificador.TemNotificacao())
            {
                return BadRequest(new { errors = Notificador.ObterNotificacoes() });
            }

            return Ok(result);
        }
    }
}


public interface INotificador
{
    bool TemNotificacao();
    List<Notificacao> ObterNotificacoes();
    void Notificar(Notificacao notificacao);
}
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