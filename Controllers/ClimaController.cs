using api.coleta.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace api.coleta.Controllers
{
    [ApiController]
    [Route("api/clima")]
    public class ClimaController : BaseController
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OpenWeatherMapSettings _settings;

        public ClimaController(
            INotificador notificador,
            IHttpClientFactory httpClientFactory,
            IOptions<OpenWeatherMapSettings> settings)
            : base(notificador)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
        }

        [HttpGet("atual")]
        [Authorize]
        public async Task<IActionResult> ObterClimaAtual([FromQuery] double lat, [FromQuery] double lon)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_settings.BaseUrl}/weather?lat={lat}&lon={lon}&appid={_settings.ApiKey}&units=metric&lang=pt_br";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, new { errors = new[] { "Erro ao consultar clima atual" } });
            }

            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }

        [HttpGet("previsao")]
        [Authorize]
        public async Task<IActionResult> ObterPrevisao([FromQuery] double lat, [FromQuery] double lon, [FromQuery] int dias = 3)
        {
            var client = _httpClientFactory.CreateClient();
            var cnt = dias * 8;
            var url = $"{_settings.BaseUrl}/forecast?lat={lat}&lon={lon}&appid={_settings.ApiKey}&units=metric&lang=pt_br&cnt={cnt}";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, new { errors = new[] { "Erro ao consultar previsão do tempo" } });
            }

            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }
    }
}
