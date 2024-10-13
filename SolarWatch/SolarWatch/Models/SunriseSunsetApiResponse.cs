namespace SolarWatch.Models
{
    // Wrapper model for the Sunrise/Sunset API response
    public class SunriseSunsetApiResponse
    {
        public SunriseSunsetResponse? Results { get; set; } // Nested results containing sunrise and sunset
        public string? Status { get; set; } // Status of the API call (e.g., "OK")
    }
}