using Newtonsoft.Json.Linq;
using SolarWatch.Models;

namespace SolarWatch.Services;

public class SunriseSunsetApiService : ISunriseSunsetService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://api.sunrise-sunset.org/json";

    public SunriseSunsetApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public SunTimes GetSunTimes(double latitude, double longitude, DateTime date)
    {
        var response = _httpClient.GetStringAsync($"{BaseUrl}?lat={latitude}&lng={longitude}&date={date:yyyy-MM-dd}&formatted=0").Result;
        var json = JObject.Parse(response);

        if (json["status"].ToString() != "OK")
        {
            throw new Exception("Failed to fetch sunrise/sunset data");
        }

        var sunriseUtc = DateTime.Parse(json["results"]["sunrise"].ToString());
        var sunsetUtc = DateTime.Parse(json["results"]["sunset"].ToString());

        return new SunTimes
        {
            SunriseUtc = sunriseUtc.ToString("hh:mm tt"),
            SunsetUtc = sunsetUtc.ToString("hh:mm tt"),
            SunriseLocal = sunriseUtc.ToLocalTime().ToString("hh:mm tt"),
            SunsetLocal = sunsetUtc.ToLocalTime().ToString("hh:mm tt")
        };
    }
}

