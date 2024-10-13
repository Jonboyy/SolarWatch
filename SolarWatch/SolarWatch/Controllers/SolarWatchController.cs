using Microsoft.AspNetCore.Mvc;
using SolarWatch.Models;
using TimeZoneConverter;


namespace SolarWatch.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SolarWatchController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        // Constructor with dependency injection for IHttpClientFactory and IConfiguration
        public SolarWatchController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }
        
        private HttpClient GetGeocodingClient()
        {
            return _httpClientFactory.CreateClient("GeocodingClient");
        }
        
        private HttpClient GetSunsetClient()
        {
            return _httpClientFactory.CreateClient("SunsetClient");
        }

        // GET endpoint that retrieves sunrise and sunset times based on city and date
        [HttpGet("{city}/{date}")]
        public async Task<IActionResult> GetSunriseSunset(string city, DateTime date)
        {
            try
            {
                // Step 1: Retrieve latitude and longitude for the given city using the Geocoding API
                var geocodingResult = await GetCoordinatesAsync(city);

                // Log the latitude and longitude for debugging
                Console.WriteLine($"City: {city}, Latitude: {geocodingResult?.Lat}, Longitude: {geocodingResult?.Lon}");

                // If the geocoding API returns null, return a BadRequest with an error message
                if (geocodingResult == null)
                {
                    return BadRequest(new { error = "City not found or misspelled. Please check the city name and try again." });
                }

                // Step 2: Retrieve sunrise and sunset times using latitude, longitude, and date
                var sunriseSunsetResult = await GetSunriseSunsetTimesAsync(geocodingResult.Lat, geocodingResult.Lon, date);

                // If the Sunrise/Sunset API returns null, indicate an issue with data retrieval
                if (sunriseSunsetResult == null)
                {
                    return BadRequest(new { error = "Unable to retrieve sunrise and sunset data. Please try again later." });
                }

                // Return the sunrise and sunset times in the response as a JSON object
                return Ok(sunriseSunsetResult);
            }
            catch (ArgumentException ex)
            {
                // Catch ArgumentException for invalid date formats and return a specific error message
                return BadRequest(new { error = ex.Message });
            }
        }

        // Helper method to call the Geocoding API and get latitude and longitude based on city name
        private async Task<GeocodingResponse> GetCoordinatesAsync(string city)
        {
            // Create a new HTTP client instance, Use the Geocoding client specifically for Geocoding API requests
            var client = GetGeocodingClient();
            var apiKey = _configuration["GeocodingAPI:ApiKey"];
            var url = $"{_configuration["GeocodingAPI:BaseUrl"]}?q={city}&limit=1&appid={apiKey}";
            Console.WriteLine($"Geocoding API Request URL: {url}");

            // Make an asynchronous GET request to the Geocoding API
            var response = await client.GetAsync(url);
            var jsonResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Geocoding API Raw Response: {jsonResponse}");

            // If the API request is unsuccessful, return null
            if (!response.IsSuccessStatusCode) return null;

            // Deserialize the JSON response into a list of GeocodingResponse objects
            var data = await response.Content.ReadFromJsonAsync<List<GeocodingResponse>>();
            if (data == null || !data.Any())
            {
                Console.WriteLine("No results found for the city.");
                return null;
            }
            var result = data?.FirstOrDefault(); // Return the first result or null if no results found

            // Log the response for debugging purposes
            Console.WriteLine($"Retrieved Latitude: {result?.Lat}, Longitude: {result?.Lon} for city: {city}");

            return result;
        }

        // Helper method to call the Sunrise/Sunset API and retrieve sunrise and sunset times
        private async Task<SunriseSunsetResponse> GetSunriseSunsetTimesAsync(double latitude, double longitude, DateTime date)
{
    // Validate date format; throw an exception if the date is invalid
    if (!DateTime.TryParse(date.ToString("yyyy-MM-dd"), out var validDate))
    {
        throw new ArgumentException("Invalid date format. Please use the format YYYY-MM-DD.");
    }

    // Create a new HTTP client instance
    var client = GetSunsetClient();

    // Format the date as a string to append to the API URL
    var dateStr = date.ToString("yyyy-MM-dd");

    // Construct the API URL with the latitude, longitude, and date for the specified city
    var url = $"{_configuration["SunriseSunsetAPI:BaseUrl"]}?lat={latitude}&lng={longitude}&date={dateStr}&formatted=0";

    // Log the full request URL for debugging purposes
    Console.WriteLine($"Requesting Sunrise/Sunset data from URL: {url}");

    // Make an asynchronous GET request to the Sunrise/Sunset API
    var response = await client.GetAsync(url);

    // Check if the response indicates a successful request (status code 200)
    if (!response.IsSuccessStatusCode) 
    {
        // Log and return null if the request was unsuccessful
        Console.WriteLine("Failed to retrieve data from Sunrise/Sunset API.");
        return null;
    }

    // Deserialize the JSON response into a SunriseSunsetApiResponse object
    var result = await response.Content.ReadFromJsonAsync<SunriseSunsetApiResponse>();

    // Check if the Results property of the response is null (indicating no data)
    if (result?.Results == null) 
    {
        // Log and return null if no data was found
        Console.WriteLine("No data found for Sunrise/Sunset times.");
        return null;
    }

    // Parse the UTC sunrise and sunset times from the API response
    var sunriseUtc = DateTime.SpecifyKind(result.Results.Sunrise, DateTimeKind.Utc);
    var sunsetUtc = DateTime.SpecifyKind(result.Results.Sunset, DateTimeKind.Utc);
    Console.WriteLine($"Raw Sunrise UTC: {sunriseUtc}, Raw Sunset UTC: {sunsetUtc}");
    
    var skipConversion = sunriseUtc.Hour > 2;
    DateTime sunriseLocal, sunsetLocal;
    if (skipConversion)
    {
        sunriseLocal = sunriseUtc;
        sunsetLocal = sunsetUtc;
    }
    else
    {
        // Determine the IANA timezone ID based on the city's latitude and longitude
        // Use TimeZoneLookup to find the IANA timezone from latitude and longitude
        var tzIana = GeoTimeZone.TimeZoneLookup.GetTimeZone(latitude, longitude).Result;
        Console.WriteLine($"Timezone ID for Coordinates: {tzIana}");
        
        // Convert the IANA timezone ID to TimeZoneInfo to perform time conversion
        var timeZoneInfo = TZConvert.GetTimeZoneInfo(tzIana);
        
        // Convert the UTC sunrise and sunset times to the local time of the specified city
        sunriseLocal = TimeZoneInfo.ConvertTimeFromUtc(sunriseUtc, timeZoneInfo);
        sunsetLocal = TimeZoneInfo.ConvertTimeFromUtc(sunsetUtc, timeZoneInfo);
    }
    
    

    

    

    // Log the final local times for debugging purposes
    Console.WriteLine($"Final Local Sunrise: {sunriseLocal}, Sunset: {sunsetLocal}");


    // Return the local sunrise and sunset times as part of the response
    return new SunriseSunsetResponse
    {
        Sunrise = sunriseLocal,
        Sunset = sunsetLocal
    };
}

    }
}
