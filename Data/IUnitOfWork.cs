namespace whtsapp.Data
{
    public class UnitOfWorkImplements : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext _context;
        public UnitOfWorkImplements(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool Commit()
        {
            try
            {
                var result = _context.SaveChanges();
                return result > 0;
            }
            catch (Exception erro)
            {
                throw erro;
            }
        }

        public async Task<bool> CommitAsync()
        {
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public void Dispose()
        {                        
        }
    }
}


public interface IUnitOfWork
{
    public bool Commit();
}