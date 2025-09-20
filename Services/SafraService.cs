using api.cliente.Models.DTOs;
using api.cliente.Repositories;
using api.coleta.Models.Entidades;
using api.coleta.Services;
using api.coleta.Utils;
using api.coleta.Utils.Maps;
using api.fazenda.models;
using api.fazenda.repositories;
using api.safra.Models.DTOs;
using api.safra.Repositories;

namespace api.safra.Services
{
    public class SafraService : ServiceBase
    {
        private readonly SafraRepository _safraRepository;
        private readonly FazendaRepository _fazendaRepository;
        private readonly ClienteRepository _clienteRepository;

        public SafraService(SafraRepository safraRepository, IUnitOfWork unitOfWork, FazendaRepository fazendaRepository, ClienteRepository clienteRepository)
            : base(unitOfWork)
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
            return safra.ToResponseDto();
        }

        public SafraResponseDTO SalvarSafra(Guid userId, SafraRequestDTO safraDto)
        {
            var safraEntidade = safraDto.ToEntity();
            if (safraEntidade == null)
            {
                throw new InvalidOperationException("Não foi possível converter os dados da safra.");
            }

            safraEntidade.UsuarioID = userId;
            _safraRepository.Adicionar(safraEntidade);
            UnitOfWork.Commit();
            return safraEntidade.ToResponseDto()!;
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

                return safra.ToResponseDto();
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
                return safra.ToResponseDto();

            }
            return null;
        }

        public PagedResult<SafraResponseDTO> ListarSafra(Guid userId, QuerySafra query)
        {
            var safra = _safraRepository.ListaSafra(userId, query);
            var safraDtos = safra.Items.ToResponseDtoList();

            foreach (var dto in safraDtos)
            {
                var fazenda = _fazendaRepository.ObterPorId(dto.FazendaID);
                if (fazenda != null)
                {
                    dto.Fazenda = fazenda.ToResponseDto();

                }
                var cliente = _clienteRepository.ObterPorId(dto.ClienteID);
                if (cliente != null)
                {
                    dto.Cliente = cliente.ToResponseDto();
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
