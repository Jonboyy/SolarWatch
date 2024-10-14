namespace SolarWatch.Models
{
    // Wrapper model for the response from the Sunrise-Sunset API
    public class SunriseSunsetApiResponse
    {
        // 'Results' property maps to the inner JSON object with sunrise and sunset times
        public SunriseSunsetResponse Results { get; set; }

        // Status of the API response, should be "OK" for successful requests
        public string Status { get; set; }
    }
}