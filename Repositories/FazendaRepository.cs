using api.fazenda.Models.Entidades;
using api.coleta.Data.Repositories;
using api.coleta.Utils;
using api.fazenda.models;
using api.coleta.Models.Entidades;
using api.coleta.Models.DTOs;

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

            if (query.ClienteID.HasValue)
                clientesQuery = clientesQuery.Where(c => c.ClienteID == query.ClienteID.Value);


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

        public List<Fazenda> ListarTodasFazendas(Guid userId)
        {
            return Context.Fazendas.Where(x => x.UsuarioID == userId).ToList();
        }

        public List<Fazenda> ListarFazendasPorUsuarioOuAdmin(Guid userId)
        {
            var usuario = Context.Usuarios.FirstOrDefault(u => u.Id == userId);

            if (usuario == null)
                return new List<Fazenda>();

            // Se o usuário tem adminId, buscar fazendas do admin
            if (usuario.adminId.HasValue)
            {
                return Context.Fazendas.Where(x => x.UsuarioID == usuario.adminId.Value).ToList();
            }

            // Caso contrário, buscar fazendas do próprio usuário
            return Context.Fazendas.Where(x => x.UsuarioID == userId).ToList();
        }

        public List<FazendaComTalhoesDTO> ListarFazendasComTalhoesPorUsuarioOuAdmin(Guid userId)
        {
            var usuario = Context.Usuarios.FirstOrDefault(u => u.Id == userId);

            if (usuario == null)
                return new List<FazendaComTalhoesDTO>();

            Guid targetUserId = usuario.adminId ?? userId;

            // Buscar fazendas do usuário
            var fazendas = Context.Fazendas
                .Where(f => f.UsuarioID == targetUserId)
                .Select(f => new
                {
                    f.Id,
                    f.Nome,
                    f.Lat,
                    f.Lng,
                    f.ClienteID
                })
                .ToList();

            // Buscar todos os talhões relacionados às fazendas em uma única query
            var fazendaIds = fazendas.Select(f => f.Id).ToList();

            var talhoesData = Context.TalhaoJson
                .Where(tj => Context.Talhoes
                    .Where(t => fazendaIds.Contains(t.FazendaID))
                    .Select(t => t.Id)
                    .Contains(tj.TalhaoID))
                .Select(tj => new
                {
                    tj.Id,
                    tj.Nome,
                    tj.Area,
                    tj.Coordenadas,
                    tj.Observacao,
                    tj.TalhaoID,
                    FazendaID = tj.Talhao.FazendaID,
                    ClienteID = tj.Talhao.ClienteID
                })
                .ToList();

            // Combinar os dados em memória
            var fazendasComTalhoes = fazendas.Select(f => new FazendaComTalhoesDTO
            {
                Id = f.Id,
                Nome = f.Nome,
                Lat = f.Lat,
                Lng = f.Lng,
                ClienteID = f.ClienteID,
                Talhoes = talhoesData
                    .Where(t => t.FazendaID == f.Id)
                    .Select(t => new TalhaoMobileDTO
                    {
                        Id = t.Id,
                        Nome = t.Nome,
                        Area = t.Area,
                        Coordenadas = t.Coordenadas,
                        Observacao = t.Observacao,
                        TalhaoID = t.TalhaoID,
                        FazendaID = t.FazendaID,
                        ClienteID = t.ClienteID
                    })
                    .ToList()
            }).ToList();

            return fazendasComTalhoes;
        }
    }
}