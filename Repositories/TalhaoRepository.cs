using api.coleta.Data.Repositories;
using api.coleta.Models.Entidades;
using api.coleta.Utils;
using api.fazenda.Models.Entidades;
using api.talhao.Models.DTOs;
using Microsoft.EntityFrameworkCore;

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

        public Talhao BuscarTalhaoSemUsuarioId(Guid id)
        {
            return Context.Talhoes.FirstOrDefault(c => c.Id == id);
        }

        public Talhao BuscarTalhaoComRelacionamentos(Guid id)
        {
            return Context.Talhoes
                .Include(t => t.Fazenda)
                .Include(t => t.Cliente)
                .FirstOrDefault(c => c.Id == id);
        }

        public PagedResult<Talhao> ListarTalhao(Guid userId, QueryTalhao query)
        {
            if (query.Page is null || query.Page < 1)
                query.Page = 1;

            int pageSize = 10;
            int page = query.Page.Value;

            var clientesQuery = Context.Talhoes
                .Where(c => c.UsuarioID == userId);

            if (query.FazendaID.HasValue)
                clientesQuery = clientesQuery.Where(c => c.FazendaID == query.FazendaID);


            int totalItems = clientesQuery.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            List<Talhao> talhoes = clientesQuery
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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

        public void AdicionarCoordenadas(TalhaoJson coordenada)
        {
            Context.TalhaoJson.Add(coordenada);
        }

        public List<TalhaoJson> BuscarTalhaoJson(Guid id)
        {
            return Context.TalhaoJson
                .Where(f => f.TalhaoID == id)
                .ToList();
        }

        public TalhaoJson BuscarTalhaoJsonPorId(Guid id)
        {
            return Context.TalhaoJson.FirstOrDefault(c => c.Id == id);
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
