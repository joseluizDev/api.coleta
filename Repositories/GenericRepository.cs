using api.coleta.Models.Entidades;
using api.coleta.Utils;
using api.fazenda.Models.Entidades;
using api.safra.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Linq;

namespace api.coleta.Data.Repositories
{
    public abstract class GenericRepository<T> where T : Entity
    {
        protected readonly ApplicationDbContext Context;

        protected readonly DbSet<T> DbSet;

        protected readonly DbConnection Connection;

        public GenericRepository(ApplicationDbContext context)
        {
            Context = context;
            DbSet = context.Set<T>();

            try
            {
                Connection = Context.Database.GetDbConnection();
            }
            catch (Exception)
            {
                Connection = null;
            }
        }
        public void Adicionar(Entity entity)
        {
            Context.Add(entity);
        }
        public void Atualizar(Entity entity)
        {
            Context.Update(entity);
        }

        public T? ObterPorId(Guid id)
        {
            return DbSet.FirstOrDefault(x => x.Id == id);
        }

        public void Deletar(Entity entity)
        {
            Context.Remove(entity);
        }

        public List<T> BuscaPaginada(IQueryable<T> query, int page = 1, int limit = 0)
        {

            if (limit < 1)
            {
                return [.. query];
            }

            int skip = (page - 1) * limit;

            return [.. query.Take(page).Skip(skip)];
        }

        public PagedResult<Safra> ListaSafra(Guid userId, QuerySafra query)
        {
            int page = query.Page ?? 1;
            if (page < 1) page = 1;
            int pageSize = 10;

            var safrasQuery = Context.Safras
                .Where(f => f.UsuarioID == userId);

            if (!string.IsNullOrWhiteSpace(query.Observacao))
                safrasQuery = safrasQuery.Where(f => f.Observacao.Contains(query.Observacao));

            if (query.DataInicio.HasValue)
                safrasQuery = safrasQuery.Where(f => f.DataInicio.Date >= query.DataInicio.Value.Date);

            if (query.DataFim.HasValue)
                safrasQuery = safrasQuery.Where(f => f.DataFim.HasValue && f.DataFim.Value.Date <= query.DataFim.Value.Date);

            if (query.FazendaID.HasValue)
                safrasQuery = safrasQuery.Where(f => f.FazendaID == query.FazendaID.Value);

            if (query.ClienteID.HasValue)
                safrasQuery = safrasQuery.Where(f => f.ClienteID == query.ClienteID.Value);

            int totalItems = safrasQuery.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var safras = safrasQuery
                .OrderBy(f => f.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<Safra>
            {
                Items = safras,
                TotalPages = totalPages,
                CurrentPage = page
            };
        }


        public Usuario ObterFuncionario(Guid id, Guid userId)
        {
            var usuario = Context.Usuarios.FirstOrDefault(x => x.Id == id && x.adminId == userId);
            return usuario; 
        }
    }
}

public interface IGenericRepository<T>
{
    public void Adicionar(Entity entity);
    public void Atualizar(Entity entity);
    T? ObterPorId(Guid id);
    List<T> BuscaPaginada(Guid id, int page = 0, int pageSize = 0);
}
