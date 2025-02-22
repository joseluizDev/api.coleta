using api.coleta.Data.Repositories;
using api.coleta.Models.Entidades;
using api.coleta.Utils;

namespace api.talhao.Repositories
{
    public class TalhaoRepository : GenericRepository<Talhao>
    {
        public TalhaoRepository(ApplicationDbContext context) : base(context)
        { }

        public void SalvarTalhoes(List<Talhao> talhoes)
        {
            foreach (var talhao in talhoes)
            {
                Adicionar(talhao);
            }
        }

        public Talhao BuscarTalhaoPorId(Guid id)
        {
            return ObterPorId(id);
        }

        public void AtualizarTalhao(Talhao talhao)
        {
            Atualizar(talhao);
        }

        public void DeletarTalhao(Talhao talhao)
        {
            Deletar(talhao);
        }

        public PagedResult<Talhao> ListarTalhao(Guid userId, int page)
        {
            if (page < 1) page = 1;
            int totalItems = Context.Talhoes.Count();
            int pageSize = 10;
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            List<Talhao> talhoes = Context.Talhoes
                .OrderBy(f => f.Id)
                .Skip(pageSize * (page - 1))
                .Take(pageSize)
                .Where(f => f.UsuarioID == userId)
                .ToList();

            return new PagedResult<Talhao>
            {
                Items = talhoes,
                TotalPages = totalPages,
                CurrentPage = page
            };
        }
    }
}
