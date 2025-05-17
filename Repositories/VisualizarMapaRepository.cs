using api.coleta.Data.Repositories;
using api.coleta.Models.DTOs;
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

        public PagedResult<Coleta> ListarVisualizarMapa(Guid userId, QueryVisualizarMap query)
        {
            if (query.Page is null or < 1) query.Page = 1;
            int page = query.Page.Value;
            int pageSize = 10;

            // Começa a construir a query base
            var queryable = Context.Coletas.AsQueryable();

            // Filtro fixo: usuário
            queryable = queryable.Where(f => f.UsuarioID == userId);

            // Filtros dinâmicos
            if (query.FuncionarioID.HasValue)
            {
                queryable = queryable.Where(f => f.UsuarioRespID == query.FuncionarioID);
            }

            if (!string.IsNullOrEmpty(query.TipoColeta))
            {
                queryable = queryable.Where(f => f.TipoColeta == Enum.Parse<TipoColeta>(query.TipoColeta));
            }

            if (!string.IsNullOrEmpty(query.TipoAnalise))
            {
                // Convert to in-memory filtering since EF can't translate List<TipoAnalise> Contains
                var tipoAnalise = Enum.Parse<TipoAnalise>(query.TipoAnalise);
                queryable = queryable.ToList()
                    .Where(f => f.TipoAnalise.Contains(tipoAnalise))
                    .AsQueryable();
            }

            // Total de itens após filtros
            int totalItems = queryable.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Aplica paginação
            List<Coleta> visualizarMapas = queryable
                .OrderBy(f => f.Id)
                .Skip(pageSize * (page - 1))
                .Take(pageSize)
                .ToList();

            return new PagedResult<Coleta>
            {
                Items = visualizarMapas,
                TotalPages = totalPages,
                CurrentPage = page
            };
        }


        public List<Coleta> ListarVisualizarMapaMobile(Guid userId)
        {
            return Context.Coletas
                .Where(x => x.UsuarioRespID == userId)
                .Where(x => !Context.Relatorios.Any(r => r.ColetaId == x.Id))
                .ToList();
        }

        public Coleta? ObterVisualizarMapaPorId(Guid id, Guid userId)
        {
            return Context.Coletas
                .Where(x => x.Id == id && x.UsuarioRespID == userId).FirstOrDefault();
        }
    }
}
