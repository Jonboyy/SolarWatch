namespace SolarWatch.Models
{
    // Model for the Sunrise/Sunset API response containing only sunrise and sunset times
    public class SunriseSunsetResponse
    {
        public DateTime Sunrise { get; set; } // Sunrise time in local city time
        public DateTime Sunset { get; set; }  // Sunset time in local city time
    }
}