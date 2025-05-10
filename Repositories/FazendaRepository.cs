using api.fazenda.Models.Entidades;
using api.coleta.Data.Repositories;
using api.coleta.Utils;
using api.fazenda.models;
using api.coleta.Models.Entidades;

namespace api.fazenda.repositories
{
    public class FazendaRepository : GenericRepository<Fazenda>
    {
        public FazendaRepository(ApplicationDbContext context) : base(context)
        { }

        public void SalvarFazendas(List<Fazenda> fazendas)
        {
            foreach (var fazenda in fazendas)
            {
                Adicionar(fazenda);
            }
        }

        public Fazenda BuscarFazendaPorId(Guid id)
        {
            return ObterPorId(id);
        }

        public void AtualizarFazenda(Fazenda fazenda)
        {
            Atualizar(fazenda);
        }

        public void DeletarFazenda(Fazenda fazenda)
        {
            Deletar(fazenda);
        }

        public Fazenda? BuscarFazendaPorId(Guid userId, Guid id)
        {
            return Context.Fazendas.FirstOrDefault(f => f.Id == id && f.UsuarioID == userId);
        }

        public PagedResult<Fazenda> ListarFazendas(Guid userId, QueryFazenda query)
        {
            if (query.Page is null || query.Page < 1)
                query.Page = 1;

            int pageSize = 10;
            int page = query.Page.Value;

            var clientesQuery = Context.Fazendas
                .Where(c => c.UsuarioID == userId);

            if (!string.IsNullOrWhiteSpace(query.Nome))
                clientesQuery = clientesQuery.Where(c => c.Nome.Contains(query.Nome));


            int totalItems = clientesQuery.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            List<Fazenda> fazendas = clientesQuery
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<Fazenda>
            {
                Items = fazendas,
                TotalPages = totalPages,
                CurrentPage = page
            };
        }

        public List<Fazenda>   ListarTodasFazendas(Guid userId)
        {
            return Context.Fazendas.Where(x => x.UsuarioID == userId ).ToList();
        }
    }
}                                              