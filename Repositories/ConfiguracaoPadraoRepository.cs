using api.coleta.Data.Repositories;
using api.coleta.Models.Entidades;

namespace api.coleta.Repositories
{
    public class ConfiguracaoPadraoRepository : GenericRepository<ConfiguracaoPadrao>
    {
        public ConfiguracaoPadraoRepository(ApplicationDbContext context) : base(context)
        { }
        public void SalvarConfiguracaoPadrao(List<ConfiguracaoPadrao> configuracoes)
        {
            foreach (var configuracao in configuracoes)
            {
                Adicionar(configuracao);
            }
        }
        public ConfiguracaoPadrao? BuscarConfiguracaoPadraoPorId(Guid id)
        {
            return ObterPorId(id);
        }
        public void AtualizarConfiguracaoPadrao(ConfiguracaoPadrao configuracao)
        {
            Atualizar(configuracao);
        }
        public void DeletarConfiguracaoPadrao(ConfiguracaoPadrao configuracao)
        {
            Deletar(configuracao);
        }

        public List<ConfiguracaoPadrao> ListConfiguracaoPadraos()
        {
           return Context.ConfiguracaoPadraos.ToList();
        }
    }
}
