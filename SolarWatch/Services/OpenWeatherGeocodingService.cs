using Newtonsoft.Json.Linq;
using SolarWatch.Models;

namespace SolarWatch.Services
{
    public class OpenWeatherGeocodingService : IGeocodingService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _apiKey;
        private const string BaseUrl = "http://api.openweathermap.org/geo/1.0/direct";

        public OpenWeatherGeocodingService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["OpenWeatherApiKey"];
        }

        public GeocodingData GetCoordinates(string city)
        {
            var response = _httpClient.GetStringAsync($"{BaseUrl}?q={city}&appid={_apiKey}").Result;
            var json = JArray.Parse(response);

            if (json.Count == 0)
            {
                throw new Exception("City not found");
            }

            return new GeocodingData
            {
                Latitude = json[0]["lat"].Value<double>(),
                Longitude = json[0]["lon"].Value<double>()
            };
        }
    }
}