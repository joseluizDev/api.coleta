namespace api.coleta.Settings
{
    public class OpenWeatherMapSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.openweathermap.org/data/2.5";
    }
}
