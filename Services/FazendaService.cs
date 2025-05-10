using api.coleta.Services;
using api.coleta.Settings;
using api.coleta.Utils;
using api.fazenda.models;
using api.fazenda.Models.Entidades;
using AutoMapper;
using Microsoft.Extensions.Options;


namespace api.fazenda.repositories
{
    public class FazendaService : ServiceBase
    {
        private readonly FazendaRepository _fazendaRepository;

        public FazendaService(FazendaRepository fazendaRepository, IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
        {
            _fazendaRepository = fazendaRepository;
        }

        public FazendaResponseDTO? BuscarFazendaPorId(Guid userId, Guid id)
        {
            var fazenda = _fazendaRepository.BuscarFazendaPorId(userId, id);

            if (fazenda != null)
            {
                return _mapper.Map<FazendaResponseDTO>(fazenda);
            }
            return null;

        }

        public PagedResult<FazendaResponseDTO> ListarFazendas(Guid userId, QueryFazenda query)
        {
            var fazendas = _fazendaRepository.ListarFazendas(userId, query);
            var fazendaDtos = _mapper.Map<List<FazendaResponseDTO>>(fazendas.Items);
            return new PagedResult<FazendaResponseDTO>
            {
                Items = fazendaDtos,
                TotalPages = fazendas.TotalPages,
                CurrentPage = fazendas.CurrentPage
            };
        }

        public FazendaResponseDTO SalvarFazendas(Guid userId, FazendaRequestDTO fazendas)
        {
            var fazendaEntidade = _mapper.Map<Fazenda>(fazendas);
            fazendaEntidade.UsuarioID = userId;
            _fazendaRepository.Adicionar(fazendaEntidade);
            UnitOfWork.Commit();
            return _mapper.Map<FazendaResponseDTO>(fazendaEntidade);
        }

        public FazendaResponseDTO? AtualizarFazenda(Guid userId, FazendaRequestDTO fazenda)
        {
            var fazendaEntidade = _mapper.Map<Fazenda>(fazenda);
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
            return _mapper.Map<FazendaResponseDTO>(obterFazenda);
        }

        public FazendaResponseDTO? DeletarFazenda(Guid userId, Guid id)
        {
            var fazenda = _fazendaRepository.BuscarFazendaPorId(userId, id);
            if (fazenda != null)
            {
                _fazendaRepository.Deletar(fazenda);
                UnitOfWork.Commit();
                return _mapper.Map<FazendaResponseDTO>(fazenda);
            }
            return null;
        }

        public List<FazendaResponseDTO> ListarTodasFazendas(Guid userId)
        {
            var fazendas = _fazendaRepository.ListarTodasFazendas(userId);
            return _mapper.Map<List<FazendaResponseDTO>>(fazendas);
        }
    }
}