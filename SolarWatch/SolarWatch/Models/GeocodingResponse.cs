namespace SolarWatch.Models
{
    // Model representing the response from the Geocoding API
    public class GeocodingResponse
    {
        // Latitude and Longitude properties to match the JSON structure
        public double Lat { get; set; }
        public double Lon { get; set; }
    }
}

