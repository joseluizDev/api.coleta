using api.fazenda.Models.Entidades;
using api.coleta.Data.Repositories;
using api.coleta.Utils;

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

        public PagedResult<Fazenda> ListarFazendas(Guid userId, int page)
        {
            if (page < 1) page = 1;

            int totalItems = Context.Fazendas.Count();
            int pageSize = 10;
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            List<Fazenda> fazendas = Context.Fazendas
                .OrderBy(f => f.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Where(f => f.UsuarioID == userId)
                .ToList();

            return new PagedResult<Fazenda>
            {
                Items = fazendas,
                TotalPages = totalPages,
                CurrentPage = page
            };
        }

        public List<Fazenda> ListarTodasFazendas(Guid userId)
        {
            return Context.Fazendas.Where(x => x.UsuarioID == userId ).ToList();
        }
    }
}