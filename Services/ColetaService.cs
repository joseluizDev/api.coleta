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

        public List<ColetasResponseDTO> BuscarColetasCidade(string nomeCidade)
        {
            try { 
            return [];
       
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
                return new List<ColetasResponseDTO>();
            }
        }

        private double CalcularRaioDaCidade(Bounds bounds)
        {
            var lat1 = bounds.Northeast.Lat;
            var lon1 = bounds.Northeast.Lng;
            var lat2 = bounds.Southwest.Lat;
            var lon2 = bounds.Southwest.Lng;
            const double R = 6371;
            var φ1 = lat1 * Math.PI / 180;
            var φ2 = lat2 * Math.PI / 180;
            var Δφ = (lat2 - lat1) * Math.PI / 180;
            var Δλ = (lon2 - lon1) * Math.PI / 180;
            var a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                    Math.Cos(φ1) * Math.Cos(φ2) *
                    Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c;
            return d * 1000 / 2;
        }
    }
}
