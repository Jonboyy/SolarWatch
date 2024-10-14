using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SolarWatch.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SolarWatch.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SolarWatchController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        // Constructor that receives IHttpClientFactory and IConfiguration via dependency injection
        public SolarWatchController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // GET endpoint to retrieve sunrise and sunset times for a given city and date
        [HttpGet("{city}/{date}")]
        public async Task<IActionResult> GetSunriseSunset(string city, string date)
        {
            // Step 1: Get the coordinates (latitude and longitude) for the specified city
            var coordinates = await GetCoordinatesAsync(city);

            // If coordinates are null, return a BadRequest indicating the city was not found
            if (coordinates == null)
            {
                return BadRequest("City not found or invalid.");
            }

            // Step 2: Get the sunrise and sunset times using the coordinates and date
            var result = await GetSunriseSunsetTimesAsync(coordinates.Lat, coordinates.Lon, date);

            // If result is null, return a BadRequest indicating data retrieval failure
            if (result == null || result.Status != "OK")
            {
                return BadRequest("Unable to retrieve sunrise and sunset data.");
            }

            // Step 3: Create a response model with the sunrise and sunset times
            var response = new SunriseSunsetResult
            {
                Sunrise = result.Results.Sunrise,
                Sunset = result.Results.Sunset
            };

            // Return the response as an OK (200) result
            return Ok(response);
        }

        // Helper method to get coordinates (latitude and longitude) for a given city
        private async Task<GeocodingResponse?> GetCoordinatesAsync(string city)
        {
            // Create an HttpClient for the Geocoding API
            var client = _httpClientFactory.CreateClient("GeocodingClient");

            // Get the API key from configuration
            var apiKey = _configuration["GeocodingAPI:ApiKey"];

            // Build the request URL with the city name and API key
            var url = $"{_configuration["GeocodingAPI:BaseUrl"]}?q={city}&limit=1&appid={apiKey}";

            // Send a GET request to the Geocoding API
            var response = await client.GetAsync(url);

            // If the response is not successful, return null
            if (!response.IsSuccessStatusCode) return null;

            // Read and deserialize the JSON response into a list of GeocodingResponse objects
            var data = await response.Content.ReadFromJsonAsync<List<GeocodingResponse>>();

            // Return the first result or null if no results were found
            return data?.FirstOrDefault();
        }

        // Helper method to get sunrise and sunset times for given coordinates and date
        private async Task<SunriseSunsetApiResponse?> GetSunriseSunsetTimesAsync(double latitude, double longitude, string date)
        {
            // Create an HttpClient for the Sunrise-Sunset API
            var client = _httpClientFactory.CreateClient("SunsetClient");

            // Build the request URL with latitude, longitude, date, and formatted=0 to get ISO 8601 format
            var url = $"{_configuration["SunriseSunsetAPI:BaseUrl"]}?lat={latitude}&lng={longitude}&date={date}&formatted=0";

            // Send a GET request to the Sunrise-Sunset API
            var response = await client.GetAsync(url);

            // If the response is not successful, return null
            if (!response.IsSuccessStatusCode) return null;

            // Read and deserialize the JSON response into a SunriseSunsetApiResponse object
            return await response.Content.ReadFromJsonAsync<SunriseSunsetApiResponse>();
        }
    }
}






