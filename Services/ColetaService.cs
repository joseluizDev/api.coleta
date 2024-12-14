using api.coleta.models;
using api.coleta.Repositories;
using api.coleta.Services;
using api.coleta.Settings;
using Microsoft.Extensions.Options;

namespace api.coleta.repositories
{
    public class ColetaService : ServiceBase
    {
        private readonly ColetaRepository _coletaRepository;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly IUnitOfWork _unitOfWork;
        public ColetaService(ColetaRepository coletaRepository, IUnitOfWork unitOfWork, HttpClient httpClient, IOptions<GoogleApiSettings> settings) : base(unitOfWork)
        {
            _coletaRepository = coletaRepository;
            _httpClient = httpClient;
            _apiKey = settings.Value.ApiKey;
            _unitOfWork = unitOfWork;
        }

    }
}
