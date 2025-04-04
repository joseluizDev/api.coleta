using api.coleta.Models.DTOs;
using api.coleta.Repositories;
using AutoMapper;

namespace api.coleta.Services
{
    public class ConfiguracaoPadraoService : ServiceBase
    {
        private readonly ConfiguracaoPadraoRepository _configuracaoPadraoRepository;
        public ConfiguracaoPadraoService(ConfiguracaoPadraoRepository configuracaoPadraoRepository, IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
        {
            _configuracaoPadraoRepository = configuracaoPadraoRepository;
        }
        public List<ConfiguracaoPadraoResponseDTO> ListarConfiguracoes()
        {
            var configuracoes = _configuracaoPadraoRepository.ListConfiguracaoPadraos();
            return _mapper.Map<List<ConfiguracaoPadraoResponseDTO>>(configuracoes);
        }
    }
}
