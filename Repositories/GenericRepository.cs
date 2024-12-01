using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace whtsapp.Data.Repositories
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
    }
}

public interface IGenericRepository<T>
{
    public void Adicionar(Entity entity);
    public void Atualizar(Entity entity);
    T? ObterPorId(Guid id);
}
