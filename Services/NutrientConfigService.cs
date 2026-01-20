using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using api.coleta.Utils.Maps;

namespace api.coleta.Services
{
    public class NutrientConfigService : ServiceBase
    {
        private readonly NutrientConfigRepository _nutrientConfigRepository;

        public NutrientConfigService(
            NutrientConfigRepository nutrientConfigRepository,
            IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _nutrientConfigRepository = nutrientConfigRepository;
        }

        public List<NutrientConfigResponseDTO> ListarNutrientConfigs(Guid? usuarioId)
        {
            var configs = _nutrientConfigRepository.ListarNutrientConfigsComFallback(usuarioId);
            return configs.ToResponseDtoList();
        }

        public NutrientConfigResponseDTO? BuscarNutrientConfigPorId(Guid id)
        {
            var config = _nutrientConfigRepository.BuscarNutrientConfigPorId(id);
            return config.ToResponseDto();
        }

        public NutrientConfigResponseDTO? BuscarNutrientConfigComFallback(string nutrientName, Guid? usuarioId)
        {
            var config = _nutrientConfigRepository.BuscarNutrientConfigComFallback(nutrientName, usuarioId);
            return config.ToResponseDto();
        }

        public NutrientConfigResponseDTO SalvarNutrientConfig(NutrientConfigRequestDTO configDTO, Guid? usuarioId = null)
        {
            var config = new NutrientConfig
            {
                UserId = configDTO.UserId ?? usuarioId,
                NutrientName = configDTO.NutrientName,
                IsGlobal = configDTO.UserId == null
            };
            config.SetConfigData(new NutrientConfigData { Ranges = configDTO.Ranges });

            _nutrientConfigRepository.SalvarNutrientConfig(config);
            UnitOfWork.Commit();

            return config.ToResponseDto()!;
        }

        public NutrientConfigResponseDTO AtualizarNutrientConfig(Guid id, NutrientConfigRequestDTO configDTO)
        {
            var config = _nutrientConfigRepository.BuscarNutrientConfigPorId(id);
            if (config == null)
            {
                throw new Exception("Configuração não encontrada");
            }

            config.NutrientName = configDTO.NutrientName;
            config.IsGlobal = configDTO.UserId == null;
            config.SetConfigData(new NutrientConfigData { Ranges = configDTO.Ranges });

            _nutrientConfigRepository.AtualizarNutrientConfig(config);
            UnitOfWork.Commit();

            return config.ToResponseDto()!;
        }

        public void DeletarNutrientConfig(Guid id)
        {
            var config = _nutrientConfigRepository.BuscarNutrientConfigPorId(id);
            if (config != null)
            {
                _nutrientConfigRepository.DeletarNutrientConfig(config);
                UnitOfWork.Commit();
            }
        }

        public List<NutrientConfigResponseDTO> ListarGlobais()
        {
            var configs = _nutrientConfigRepository.ListarGlobais();
            return configs.ToResponseDtoList();
        }

        public List<NutrientConfigResponseDTO> ListarPersonalizadas(Guid userId)
        {
            var configs = _nutrientConfigRepository.ListarPersonalizadas(userId);
            return configs.ToResponseDtoList();
        }

        public void DeletarGlobal(Guid id)
        {
            var config = _nutrientConfigRepository.BuscarNutrientConfigPorId(id);
            if (config == null || !config.IsGlobal)
            {
                throw new Exception("Configuração global não encontrada.");
            }
            _nutrientConfigRepository.DeletarNutrientConfig(config);
            UnitOfWork.Commit();
        }

        public void DeletarPersonalizada(Guid id, Guid userId)
        {
            var config = _nutrientConfigRepository.BuscarNutrientConfigPorId(id);
            if (config == null || config.IsGlobal || config.UserId != userId)
            {
                throw new Exception("Configuração personalizada não encontrada ou acesso negado.");
            }
            _nutrientConfigRepository.DeletarNutrientConfig(config);
            UnitOfWork.Commit();
        }

        public NutrientConfigResponseDTO? BuscarPersonalizadaPorNome(string nutrientName, Guid userId)
        {
            var config = _nutrientConfigRepository.BuscarNutrientConfigPersonalizadaPorNomeEUsuario(nutrientName, userId);
            return config.ToResponseDto();
        }
    }
}