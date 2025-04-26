using api.coleta.Data.Repositories;
using api.coleta.Models.Entidades;
using api.coleta.Utils;

namespace api.coleta.Repositories
{
    public class VisualizarMapaRepository : GenericRepository<Coleta>
    {
        public VisualizarMapaRepository(ApplicationDbContext context) : base(context)
        { }

        public void SalvarVisualizarMapa(Coleta visualizarMapa)
        {
            Adicionar(visualizarMapa);
        }

        public Coleta BuscarVisualizarMapaPorId(Guid userId, Guid id)
        {
            return Context.Coletas.FirstOrDefault(c => c.Id == id && c.UsuarioID == userId);
        }

        public void AtualizarVisualizarMapa(Coleta visualizarMapa)
        {
            Atualizar(visualizarMapa);
        }

        public void DeletarVisualizarMapa(Coleta visualizarMapa)
        {
            Deletar(visualizarMapa);
        }

        public Coleta DeletarVisualizarMapaPorId(Guid id)
        {
            return Context.Coletas.FirstOrDefault(c => c.Id == id);
        }

        public Coleta BuscarVisualizarMapaId(Guid idUser, Guid id)
        {
            return Context.Coletas.FirstOrDefault(c => c.Id == id && c.UsuarioID == idUser);
        }

        public Coleta BuscarVisualizarMapaPorIdTalhao(Guid idUser, Guid id)
        {
            return Context.Coletas.FirstOrDefault(c => c.TalhaoID == id && c.UsuarioID == idUser);
        }

        public PagedResult<Coleta> ListarVisualizarMapa(Guid userId, int page)
        {
            {
                if (page < 1) page = 1;
                int totalItems = Context.Coletas.Count();
                int pageSize = 10;
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                List<Coleta> visualizarMapas = Context.Coletas
                    .OrderBy(f => f.Id)
                    .Skip(pageSize * (page - 1))
                    .Take(pageSize)
                    .Where(f => f.UsuarioID == userId)
                    .ToList();

                return new PagedResult<Coleta>
                {
                    Items = visualizarMapas,
                    TotalPages = totalPages,
                    CurrentPage = page
                };
            }
        }

        public List<Coleta> ListarVisualizarMapaMobile(Guid userId)
        {
            return Context.Coletas
                .Where(x => x.UsuarioRespID == userId)
                .Where(x => !Context.Relatorios.Any(r => r.ColetaId == x.Id))
                .ToList();
        }
    }
}