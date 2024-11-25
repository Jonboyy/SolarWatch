using Newtonsoft.Json.Linq;
using SolarWatch.Models;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace SolarWatch.Services
{
    public class SunriseSunsetApiService : ISunriseSunsetService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.sunrise-sunset.org/json";

        public SunriseSunsetApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<SunTimes> GetSunTimesAsync(double latitude, double longitude, DateTime date, TimeZoneInfo timeZone)
        {
            var url = $"{BaseUrl}?lat={latitude.ToString(CultureInfo.InvariantCulture)}&lng={longitude.ToString(CultureInfo.InvariantCulture)}&date={date:yyyy-MM-dd}&formatted=0";
            var response = await _httpClient.GetStringAsync(url);

            Console.WriteLine($"API Response: {response}");

            var json = JObject.Parse(response);

            if (json["status"]?.ToString() != "OK")
            {
                throw new Exception("Failed to fetch sunrise/sunset data");
            }

            var sunriseStr = json["results"]?["sunrise"]?.ToString();
            var sunsetStr = json["results"]?["sunset"]?.ToString();

            Console.WriteLine($"Raw Sunrise String: {sunriseStr}");
            Console.WriteLine($"Raw Sunset String: {sunsetStr}");

            if (string.IsNullOrEmpty(sunriseStr) || string.IsNullOrEmpty(sunsetStr))
            {
                throw new Exception("Sunrise or sunset time is missing in the API response.");
            }

            DateTime sunriseUtc, sunsetUtc;
            
            if (!DateTime.TryParseExact(sunriseStr, "yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out sunriseUtc))
            {
                if (!DateTime.TryParseExact(sunriseStr, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out sunriseUtc))
                {
                    throw new Exception($"Failed to parse sunrise time: {sunriseStr}");
                }
                else
                {
                    sunriseUtc = DateTime.SpecifyKind(sunriseUtc, DateTimeKind.Utc);
                }
            }

            if (!DateTime.TryParseExact(sunsetStr, "yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out sunsetUtc))
            {
                if (!DateTime.TryParseExact(sunsetStr, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out sunsetUtc))
                {
                    throw new Exception($"Failed to parse sunset time: {sunsetStr}");
                }
                else
                {
                    
                    sunsetUtc = DateTime.SpecifyKind(sunsetUtc, DateTimeKind.Utc);
                }
            }

            
            if (sunriseUtc.Kind != DateTimeKind.Utc)
            {
                sunriseUtc = DateTime.SpecifyKind(sunriseUtc, DateTimeKind.Utc);
            }

            if (sunsetUtc.Kind != DateTimeKind.Utc)
            {
                sunsetUtc = DateTime.SpecifyKind(sunsetUtc, DateTimeKind.Utc);
            }

            
            var sunriseLocal = TimeZoneInfo.ConvertTimeFromUtc(sunriseUtc, timeZone).AddHours(-1);
            var sunsetLocal = TimeZoneInfo.ConvertTimeFromUtc(sunsetUtc, timeZone).AddHours(-1);


            return new SunTimes
            {
                SunriseUtc = sunriseUtc.ToString("hh:mm tt", CultureInfo.InvariantCulture),
                SunsetUtc = sunsetUtc.ToString("hh:mm tt", CultureInfo.InvariantCulture),
                SunriseLocal = sunriseLocal.ToString("hh:mm tt", CultureInfo.InvariantCulture),
                SunsetLocal = sunsetLocal.ToString("hh:mm tt", CultureInfo.InvariantCulture)
            };
        }
    }
}








