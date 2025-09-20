namespace api.coleta.Services
{
    public abstract class ServiceBase
    {
        protected IUnitOfWork UnitOfWork;

        protected ServiceBase(IUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }
    }
}
