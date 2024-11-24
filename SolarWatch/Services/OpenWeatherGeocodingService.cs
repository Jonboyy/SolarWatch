using Newtonsoft.Json.Linq;

namespace SolarWatch.Services;

public class OpenWeatherGeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private const string ApiKey = "YOUR_OPENWEATHER_API_KEY";
    private const string BaseUrl = "http://api.openweathermap.org/geo/1.0/direct";

    public OpenWeatherGeocodingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public (double Latitude, double Longitude) GetCoordinates(string city)
    {
        var response = _httpClient.GetStringAsync($"{BaseUrl}?q={city}&appid={ApiKey}").Result;
        var json = JArray.Parse(response);
        if (json.Count == 0)
            throw new Exception("City not found");

        return (json[0]["lat"].Value<double>(), json[0]["lon"].Value<double>());
    }
}