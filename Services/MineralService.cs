using System.Threading.Tasks;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using AutoMapper;

namespace api.coleta.Services
{
    public class MineralService : ServiceBase
    {
        private readonly MineralRepository _mineralRepository;
        public MineralService(MineralRepository mineralRepository, IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
        {
            _mineralRepository = mineralRepository;
        }
        public async Task<List<Minerais>> ListarMinerais()
        {
            return await _mineralRepository.ListarMinerais();
        }
        public void AdicionarMineral(Minerais mineral)
        {
            _mineralRepository.Adicionar(mineral);
            UnitOfWork.Commit();
        }
        public void AtualizarMineral(Minerais mineral)
        {
            _mineralRepository.Atualizar(mineral);
            UnitOfWork.Commit();
        }
        public void DeletarMineral(Minerais mineral)
        {
            _mineralRepository.Deletar(mineral);
            UnitOfWork.Commit();
        }
    }
}
