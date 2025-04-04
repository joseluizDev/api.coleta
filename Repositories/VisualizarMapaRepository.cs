using api.coleta.Data.Repositories;
using api.coleta.Models.Entidades;
using api.coleta.Utils;

namespace api.coleta.Repositories
{
    public class VisualizarMapaRepository : GenericRepository<VisualizarMapa>
    {
        public VisualizarMapaRepository(ApplicationDbContext context) : base(context)
        { }

        public void SalvarVisualizarMapa(VisualizarMapa visualizarMapa)
        {
            Adicionar(visualizarMapa);
        }

        public VisualizarMapa BuscarVisualizarMapaPorId(Guid id)
        {
            return ObterPorId(id);
        }

        public void AtualizarVisualizarMapa(VisualizarMapa visualizarMapa)
        {
            Atualizar(visualizarMapa);
        }

        public void DeletarVisualizarMapa(VisualizarMapa visualizarMapa)
        {
            Deletar(visualizarMapa);
        }

        public VisualizarMapa DeletarVisualizarMapaPorId(Guid id)
        {
            return Context.VisualizarMapas.FirstOrDefault(c => c.Id == id);
        }

        public VisualizarMapa BuscarVisualizarMapaId(Guid idUser, Guid id)
        {
            return Context.VisualizarMapas.FirstOrDefault(c => c.Id == id && c.UsuarioID == idUser);
        }

        public VisualizarMapa BuscarVisualizarMapaPorIdTalhao(Guid idUser, Guid id)
        {
            return Context.VisualizarMapas.FirstOrDefault(c => c.TalhaoID == id && c.UsuarioID == idUser);
        }

        public PagedResult<VisualizarMapa> ListarVisualizarMapa(Guid userId, int page)
        {
            {
                if (page < 1) page = 1;
                int totalItems = Context.VisualizarMapas.Count();
                int pageSize = 10;
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                List<VisualizarMapa> visualizarMapas = Context.VisualizarMapas
                    .OrderBy(f => f.Id)
                    .Skip(pageSize * (page - 1))
                    .Take(pageSize)
                    .Where(f => f.UsuarioID == userId)
                    .ToList();

                return new PagedResult<VisualizarMapa>
                {
                    Items = visualizarMapas,
                    TotalPages = totalPages,
                    CurrentPage = page
                };
            }
        }
    }
}
