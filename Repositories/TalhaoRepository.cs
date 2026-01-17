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

        public List<Talhao> ListarTalhao(Guid userId, QueryTalhao query)
        {
            var clientesQuery = Context.Talhoes
                .Where(c => c.UsuarioID == userId);

            if (query.FazendaID.HasValue)
                clientesQuery = clientesQuery.Where(c => c.FazendaID == query.FazendaID);

            List<Talhao> talhoes = clientesQuery
                .OrderBy(c => c.Id)
                .ToList();

            return talhoes;
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
            return Context.TalhaoJson
                .AsNoTracking()
                .FirstOrDefault(c => c.Id == id);
        }

        public TalhaoJson DeletarTalhaoPorId(Guid id)
        {
            return Context.TalhaoJson
                .Where(f => f.TalhaoID == id)
                .FirstOrDefault();
        }

        public void DeletarTalhaoJson(TalhaoJson talhaoJson)
        {
            // Reattach se necessário
            if (Context.Entry(talhaoJson).State == EntityState.Detached)
            {
                Context.TalhaoJson.Attach(talhaoJson);
            }
            Context.TalhaoJson.Remove(talhaoJson);
        }

        public void AtualizarTalhaoJson(TalhaoJson talhaoJson)
        {
            Context.TalhaoJson.Update(talhaoJson);
        }

        public Talhao? BuscarTalhaoPorFazendaID(Guid userID, Guid id)
        {
            return Context.Talhoes.Where(item => item.FazendaID == id && item.UsuarioID == userID).FirstOrDefault();
        }

        public List<Talhao> ListarTodosComFazenda(Guid userId, Guid? fazendaId = null)
        {
            var query = Context.Talhoes
                .Include(t => t.Fazenda)
                .Where(t => t.UsuarioID == userId);

            if (fazendaId.HasValue)
            {
                query = query.Where(t => t.FazendaID == fazendaId.Value);
            }

            return query.ToList();
        }

        /// <summary>
        /// Calcula o total de hectares utilizados por um cliente
        /// </summary>
        public async Task<decimal> ObterTotalHectaresPorClienteAsync(Guid clienteId)
        {
            // Get all talhoes for this cliente
            var talhoes = await Context.Talhoes
                .Where(t => t.ClienteID == clienteId)
                .Select(t => t.Id)
                .ToListAsync();

            if (!talhoes.Any())
            {
                return 0;
            }

            // Get all TalhaoJson for these talhoes and sum the area
            var talhoesJson = await Context.TalhaoJson
                .Where(tj => talhoes.Contains(tj.TalhaoID))
                .ToListAsync();

            decimal totalHectares = 0;
            foreach (var tj in talhoesJson)
            {
                if (!string.IsNullOrEmpty(tj.Area))
                {
                    // Try to parse the area value
                    var areaStr = tj.Area.Replace(",", ".").Trim();
                    if (decimal.TryParse(areaStr, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var area))
                    {
                        totalHectares += area;
                    }
                }
            }

            return totalHectares;
        }
    }
}
