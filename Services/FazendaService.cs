using api.coleta.Services;
using api.coleta.Settings;
using api.coleta.Utils;
using api.coleta.Utils.Maps;
using api.fazenda.models;
using api.fazenda.Models.Entidades;
using Microsoft.Extensions.Options;


namespace api.fazenda.repositories
{
    public class FazendaService : ServiceBase
    {
        private readonly FazendaRepository _fazendaRepository;

        public FazendaService(FazendaRepository fazendaRepository, IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _fazendaRepository = fazendaRepository;
        }

        public FazendaResponseDTO? BuscarFazendaPorId(Guid userId, Guid id)
        {
            var fazenda = _fazendaRepository.BuscarFazendaPorId(userId, id);

            if (fazenda != null)
            {
                return fazenda.ToResponseDto();
            }
            return null;

        }

        public PagedResult<FazendaResponseDTO> ListarFazendas(Guid userId, QueryFazenda query)
        {
            var fazendas = _fazendaRepository.ListarFazendas(userId, query);
            var fazendaDtos = fazendas.Items.ToResponseDtoList();
            return new PagedResult<FazendaResponseDTO>
            {
                Items = fazendaDtos,
                TotalPages = fazendas.TotalPages,
                CurrentPage = fazendas.CurrentPage
            };
        }

        public FazendaResponseDTO SalvarFazendas(Guid userId, FazendaRequestDTO fazendas)
        {
            var fazendaEntidade = fazendas.ToEntity();
            if (fazendaEntidade == null)
            {
                throw new InvalidOperationException("Não foi possível converter os dados da fazenda.");
            }

            fazendaEntidade.UsuarioID = userId;
            _fazendaRepository.Adicionar(fazendaEntidade);
            UnitOfWork.Commit();
            return fazendaEntidade.ToResponseDto()!;
        }

        public FazendaResponseDTO? AtualizarFazenda(Guid userId, FazendaRequestDTO fazenda)
        {
            var fazendaEntidade = fazenda.ToEntity();
            if (fazendaEntidade == null)
            {
                throw new InvalidOperationException("Não foi possível converter os dados da fazenda.");
            }
            var obterFazenda = _fazendaRepository.BuscarFazendaPorId(userId, fazendaEntidade.Id);
            if (obterFazenda != null)
            {
                obterFazenda.Nome = fazendaEntidade.Nome;
                obterFazenda.Lat = fazendaEntidade.Lat;
                obterFazenda.Lng = fazendaEntidade.Lng;
                obterFazenda.ClienteID = fazendaEntidade.ClienteID;
                _fazendaRepository.Atualizar(obterFazenda);
                UnitOfWork.Commit();
            }
            return obterFazenda.ToResponseDto();
        }

        public FazendaResponseDTO? DeletarFazenda(Guid userId, Guid id)
        {
            var fazenda = _fazendaRepository.BuscarFazendaPorId(userId, id);
            if (fazenda != null)
            {
                _fazendaRepository.Deletar(fazenda);
                UnitOfWork.Commit();
                return fazenda.ToResponseDto();
            }
            return null;
        }

        public List<FazendaResponseDTO> ListarTodasFazendas(Guid userId)
        {
            var fazendas = _fazendaRepository.ListarTodasFazendas(userId);
            return fazendas.ToResponseDtoList();
        }

        public List<FazendaResponseDTO> ListarFazendasPorUsuarioOuAdmin(Guid userId)
        {
            var fazendas = _fazendaRepository.ListarFazendasPorUsuarioOuAdmin(userId);
            return fazendas.ToResponseDtoList();
        }

        public List<object> ListarFazendasComTalhoesPorUsuarioOuAdmin(Guid userId)
        {
            return _fazendaRepository.ListarFazendasComTalhoesPorUsuarioOuAdmin(userId);
        }
    }
}
