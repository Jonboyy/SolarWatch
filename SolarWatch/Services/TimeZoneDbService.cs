using System;
using System.Globalization;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SolarWatch.Services;
using TimeZoneConverter;

namespace SolarWatch.Services
{
    public class TimeZoneDbService : ITimeZoneService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string BaseUrl = "http://api.timezonedb.com/v2.1/get-time-zone";

        public TimeZoneDbService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["TimeZoneDbApiKey"] ?? throw new Exception("TimeZoneDbApiKey is not configured.");
        }

        public async Task<TimeZoneInfo> GetTimeZoneAsync(double latitude, double longitude)
        {
            var query = $"key={Uri.EscapeDataString(_apiKey)}&format=json&by=position&lat={latitude.ToString(CultureInfo.InvariantCulture)}&lng={longitude.ToString(CultureInfo.InvariantCulture)}";
            var url = $"{BaseUrl}?{query}";

            var response = await _httpClient.GetStringAsync(url);
            var json = JObject.Parse(response);

            if (json["status"]?.ToString() != "OK")
            {
                var message = json["message"]?.ToString() ?? "Unknown error";
                throw new Exception($"TimeZoneDB API error: {message}");
            }

            var timeZoneId = json["zoneName"]?.ToString();

            if (string.IsNullOrEmpty(timeZoneId))
            {
                throw new Exception("The 'zoneName' field is missing or empty in the API response.");
            }

            TimeZoneInfo timeZone;

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var windowsTimeZoneId = TZConvert.IanaToWindows(timeZoneId);
                    timeZone = TimeZoneInfo.FindSystemTimeZoneById(windowsTimeZoneId);
                }
                else
                {
                    timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to find time zone for ID '{timeZoneId}'.", ex);
            }

            return timeZone;
        }
    }
}


