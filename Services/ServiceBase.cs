using AutoMapper;

namespace api.coleta.Services
{
    public abstract class ServiceBase
    {
        protected IUnitOfWork UnitOfWork;
        protected IMapper _mapper;
        protected ServiceBase(IUnitOfWork unitOfWork, IMapper mapper)
        {
            UnitOfWork = unitOfWork;
            _mapper = mapper;
        }
    }
}
