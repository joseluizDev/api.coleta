using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using api.coleta.Utils.Maps;

namespace api.coleta.Services
{
    public class ConfiguracaoPersonalizadaService : ServiceBase
    {
        private readonly ConfiguracaoPersonalizadaRepository _configuracaoPersonalizadaRepository;

        public ConfiguracaoPersonalizadaService(
            ConfiguracaoPersonalizadaRepository configuracaoPersonalizadaRepository,
            IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _configuracaoPersonalizadaRepository = configuracaoPersonalizadaRepository;
        }

        public List<ConfiguracaoPersonalizadaResponseDTO> ListarConfiguracoesPersonalizadas(Guid usuarioId)
        {
            var configuracoes = _configuracaoPersonalizadaRepository.ListarConfiguracoesPersonalizadasPorUsuario(usuarioId);
            return configuracoes.ToResponseDtoList();
        }

        public ConfiguracaoPersonalizadaResponseDTO? BuscarConfiguracaoPersonalizadaPorId(Guid id, Guid usuarioId)
        {
            var configuracao = _configuracaoPersonalizadaRepository.BuscarConfiguracaoPersonalizadaPorIdEUsuario(id, usuarioId);
            if (configuracao == null)
            {
                return null;
            }
            return configuracao.ToResponseDto();
        }

        public ConfiguracaoPersonalizadaResponseDTO SalvarConfiguracaoPersonalizada(ConfiguracaoPersonalizadaRequestDTO configuracaoDTO, Guid usuarioId)
        {
            var configuracao = new ConfiguracaoPersonalizada
            {
                Nome = configuracaoDTO.Nome,
                LimiteInferior = configuracaoDTO.LimiteInferior,
                LimiteSuperior = configuracaoDTO.LimiteSuperior,
                CorHex = configuracaoDTO.CorHex,
                UsuarioId = usuarioId
            };

            _configuracaoPersonalizadaRepository.SalvarConfiguracaoPersonalizada(configuracao);
            UnitOfWork.Commit();

            return configuracao.ToResponseDto()!;
        }

        public ConfiguracaoPersonalizadaResponseDTO? AtualizarConfiguracaoPersonalizada(Guid id, ConfiguracaoPersonalizadaRequestDTO configuracaoDTO, Guid usuarioId)
        {
            var configuracaoExistente = _configuracaoPersonalizadaRepository.BuscarConfiguracaoPersonalizadaPorIdEUsuario(id, usuarioId);
            if (configuracaoExistente == null)
            {
                return null;
            }

            configuracaoExistente.Nome = configuracaoDTO.Nome;
            configuracaoExistente.LimiteInferior = configuracaoDTO.LimiteInferior;
            configuracaoExistente.LimiteSuperior = configuracaoDTO.LimiteSuperior;
            configuracaoExistente.CorHex = configuracaoDTO.CorHex;

            _configuracaoPersonalizadaRepository.AtualizarConfiguracaoPersonalizada(configuracaoExistente);
            UnitOfWork.Commit();

            return configuracaoExistente.ToResponseDto();
        }

        public bool DeletarConfiguracaoPersonalizada(Guid id, Guid usuarioId)
        {
            var configuracao = _configuracaoPersonalizadaRepository.BuscarConfiguracaoPersonalizadaPorIdEUsuario(id, usuarioId);
            if (configuracao == null)
            {
                return false;
            }

            _configuracaoPersonalizadaRepository.DeletarConfiguracaoPersonalizada(configuracao);
            return UnitOfWork.Commit();
        }
    }
}
