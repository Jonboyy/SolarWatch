namespace SolarWatch.Models
{
    // Model for the Geocoding API response containing latitude and longitude
    public class GeocodingResponse
    {
        public double Lat { get; set; } // Note: This should match the exact field names in the JSON
        public double Lon { get; set; }
    }
}
