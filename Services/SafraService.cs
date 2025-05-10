using api.cliente.Models.DTOs;
using api.cliente.Repositories;
using api.coleta.Models.Entidades;
using api.coleta.Services;
using api.coleta.Utils;
using api.fazenda.models;
using api.fazenda.repositories;
using api.safra.Models.DTOs;
using api.safra.Repositories;
using AutoMapper;

namespace api.safra.Services
{
    public class SafraService : ServiceBase
    {
        private readonly SafraRepository _safraRepository;
        private readonly FazendaRepository _fazendaRepository;
        private readonly ClienteRepository _clienteRepository;

        public SafraService(SafraRepository safraRepository, IUnitOfWork unitOfWork, IMapper mapper, FazendaRepository fazendaRepository, ClienteRepository clienteRepository)
            : base(unitOfWork, mapper)
        {
            _safraRepository = safraRepository;
            _fazendaRepository = fazendaRepository;
            _clienteRepository = clienteRepository;
        }

        public SafraResponseDTO? BuscarSafraPorId(Guid? userId, Guid id)
        {
            var safra = _safraRepository.ObterPorId(id);
            if (safra == null)
            {
                return null;
            }
            return _mapper.Map<SafraResponseDTO>(safra);
        }

        public SafraResponseDTO SalvarSafra(Guid userId, SafraRequestDTO safraDto)
        {
            var safraEntidade = _mapper.Map<Safra>(safraDto);
            safraEntidade.UsuarioID = userId;
            _safraRepository.Adicionar(safraEntidade);
            UnitOfWork.Commit();
            return _mapper.Map<SafraResponseDTO>(safraEntidade);
        }

        public SafraResponseDTO? AtualizarSafra(Guid userId, SafraRequestDTO safraDto)
        {
            var safra = _safraRepository.BuscarSafraId(userId, (Guid)safraDto.Id);

            if (safra != null)
            {
                safra.FazendaID = safraDto.FazendaID;
                safra.ClienteID = safraDto.ClienteID;
                safra.DataFim = safraDto.DataFim;
                safra.DataInicio = safraDto.DataInicio;
                safra.Observacao = safraDto.Observacao;

                _safraRepository.Atualizar(safra);
                UnitOfWork.Commit();

                return _mapper.Map<SafraResponseDTO>(safra);
            }

            return null;
        }

        public SafraResponseDTO? DeletarSafra(Guid userId, Guid id)
        {
            var safra = _safraRepository.BuscarSafraId(userId, id);
            if (safra != null)
            {
                _safraRepository.Deletar(safra);
                UnitOfWork.Commit();
                return _mapper.Map<SafraResponseDTO>(safra);

            }
            return null;
        }

        public PagedResult<SafraResponseDTO> ListarSafra(Guid userId, QuerySafra query)
        {
            var safra = _safraRepository.ListaSafra(userId, query);
            var safraDtos = _mapper.Map<List<SafraResponseDTO>>(safra.Items);

            foreach (var dto in safraDtos)
            {
                var fazenda = _fazendaRepository.ObterPorId(dto.FazendaID);
                if (fazenda != null)
                {
                    dto.Fazenda = _mapper.Map<FazendaResponseDTO>(fazenda);

                }
                var cliente = _clienteRepository.ObterPorId(dto.ClienteID);
                if (cliente != null)
                {
                    dto.Cliente = _mapper.Map<ClienteResponseDTO>(cliente);
                }
            }

            return new PagedResult<SafraResponseDTO>
            {
                Items = safraDtos,
                TotalPages = safra.TotalPages,
                CurrentPage = safra.CurrentPage
            };
        }
    }
}
