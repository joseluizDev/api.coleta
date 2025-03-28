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

        public Talhao BuscarTalhaoId(Guid idUser, Guid id)
        {
            return Context.Talhoes.FirstOrDefault(c => c.Id == id && c.UsuarioID == idUser);
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
         
        public TalhaoJson BuscarPorTalhao(Guid id)
        {
            return Context.TalhaoJson.FirstOrDefault(c => c.Id == id);
        }

        public void AdicionarCoordenadas(TalhaoJson coordenada){
            Context.TalhaoJson.Add(coordenada);
        }

        public List<TalhaoJson> BuscarTalhaoJson(Guid id)
        {
            return Context.TalhaoJson
                .Where(f => f.TalhaoID == id)
                .ToList();
        }

        public TalhaoJson DeletarTalhaoPorId(Guid id)
        {
            return Context.TalhaoJson
                .Where(f => f.TalhaoID == id)
                .FirstOrDefault();
        }

        public Talhao? BuscarTalhaoPorFazendaID(Guid userID, Guid id)
        {
            return Context.Talhoes.Where(item => item.FazendaID == id && item.UsuarioID == userID).FirstOrDefault();
        }
    }
}
