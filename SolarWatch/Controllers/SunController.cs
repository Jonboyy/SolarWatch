using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace SolarWatch.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SunController : ControllerBase
{
    private const string GeocodingApiKey = "70bbaac8774cb67168031dc6c5b12235";
    private const string GeocodingApiUrl = "http://api.openweathermap.org/geo/1.0/direct";
    private const string SunriseSunsetApiUrl = "https://api.sunrise-sunset.org/json";

    [HttpGet]
    public IActionResult GetSunTimes([FromQuery] string city, [FromQuery] string date = null, [FromQuery] string timezone = "local")
    {
        if (string.IsNullOrEmpty(city))
        {
            return BadRequest(new
            {
                error = "Missing parameter",
                details = "The 'city' parameter is required."
            });
        }

        try
        {
            var (latitude, longitude) = GetCoordinates(city);
            
            var targetDate = date ?? DateTime.UtcNow.ToString("yyyy-MM-dd");
            
            if (!DateTime.TryParse(targetDate, out DateTime parsedDate))
            {
                return BadRequest(new
                {
                    error = "Invalid date format",
                    details = "Please provide the date in YYYY-MM-DD format."
                });
            }
            
            var sunTimes = GetSunTimes(latitude, longitude, parsedDate);
            
            return Ok(new
            {
                city,
                date = targetDate,
                timezone,
                sunrise = timezone == "utc" ? sunTimes.SunriseUtc : sunTimes.SunriseLocal,
                sunset = timezone == "utc" ? sunTimes.SunsetUtc : sunTimes.SunsetLocal
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                error = "Internal Server Error",
                details = ex.Message
            });
        }
    }

    private (double Latitude, double Longitude) GetCoordinates(string city)
    {
        using var client = new HttpClient();
        var response = client.GetStringAsync($"{GeocodingApiUrl}?q={city}&appid={GeocodingApiKey}").Result;

        var json = JArray.Parse(response);
        if (json.Count == 0)
        {
            throw new Exception("City not found");
        }

        var lat = json[0]["lat"].Value<double>();
        var lon = json[0]["lon"].Value<double>();
        return (lat, lon);
    }

    private (string SunriseUtc, string SunsetUtc, string SunriseLocal, string SunsetLocal) GetSunTimes(double lat, double lon, DateTime date)
    {
        using var client = new HttpClient();
        var response = client.GetStringAsync($"{SunriseSunsetApiUrl}?lat={lat}&lng={lon}&date={date:yyyy-MM-dd}&formatted=0").Result;

        var json = JObject.Parse(response);
        if (json["status"].ToString() != "OK")
        {
            throw new Exception("Failed to fetch sunrise/sunset data");
        }

        var sunriseUtc = DateTime.Parse(json["results"]["sunrise"].ToString());
        var sunsetUtc = DateTime.Parse(json["results"]["sunset"].ToString());

        var sunriseLocal = sunriseUtc.ToLocalTime();
        var sunsetLocal = sunsetUtc.ToLocalTime();

        return (sunriseUtc.ToString("hh:mm tt"), sunsetUtc.ToString("hh:mm tt"), sunriseLocal.ToString("hh:mm tt"), sunsetLocal.ToString("hh:mm tt"));
    }
}
