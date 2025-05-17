using api.coleta.Data.Repositories;
using api.coleta.Models.Entidades;
using Microsoft.EntityFrameworkCore;

namespace api.coleta.Repositories
{
    public class ConfiguracaoPersonalizadaRepository : GenericRepository<ConfiguracaoPersonalizada>
    {
        public ConfiguracaoPersonalizadaRepository(ApplicationDbContext context) : base(context)
        { }

        public void SalvarConfiguracaoPersonalizada(ConfiguracaoPersonalizada configuracao)
        {
            Adicionar(configuracao);
        }

        public ConfiguracaoPersonalizada? BuscarConfiguracaoPersonalizadaPorId(Guid id)
        {
            return ObterPorId(id);
        }

        public void AtualizarConfiguracaoPersonalizada(ConfiguracaoPersonalizada configuracao)
        {
            Atualizar(configuracao);
        }

        public void DeletarConfiguracaoPersonalizada(ConfiguracaoPersonalizada configuracao)
        {
            Deletar(configuracao);
        }

        public List<ConfiguracaoPersonalizada> ListarConfiguracoesPersonalizadasPorUsuario(Guid usuarioId)
        {
            return Context.ConfiguracaoPersonalizadas
                .Where(c => c.UsuarioId == usuarioId)
                .ToList();
        }

        public ConfiguracaoPersonalizada? BuscarConfiguracaoPersonalizadaPorIdEUsuario(Guid id, Guid usuarioId)
        {
            return Context.ConfiguracaoPersonalizadas
                .FirstOrDefault(c => c.Id == id && c.UsuarioId == usuarioId);
        }
    }
}
