using api.coleta.Models.DTOs;
using api.coleta.Repositories;
using api.coleta.Utils.Maps;

namespace api.coleta.Services
{
    public class ConfiguracaoPadraoService : ServiceBase
    {
        private readonly ConfiguracaoPadraoRepository _configuracaoPadraoRepository;
        public ConfiguracaoPadraoService(ConfiguracaoPadraoRepository configuracaoPadraoRepository, IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _configuracaoPadraoRepository = configuracaoPadraoRepository;
        }
        public List<ConfiguracaoPadraoResponseDTO> ListarConfiguracoes()
        {
            var configuracoes = _configuracaoPadraoRepository.ListConfiguracaoPadraos();
            return configuracoes.ToResponseDtoList();
        }
    }
}
