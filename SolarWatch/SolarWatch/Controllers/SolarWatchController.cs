using Microsoft.AspNetCore.Mvc;
using SolarWatch.Models;


namespace SolarWatch.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SolarWatchController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
        
    public SolarWatchController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }
        
    [HttpGet("{city}/{date}")]
    public async Task<IActionResult> GetSunriseSunset(string city, string date)
    {
        var coordinates = await GetCoordinatesAsync(city);
            
        if (coordinates == null)
        {
            return BadRequest("City not found or invalid.");
        }
            
        var result = await GetSunriseSunsetTimesAsync(coordinates.Lat, coordinates.Lon, date);
            
        if (result == null || result.Status != "OK")
        {
            return BadRequest("Unable to retrieve sunrise and sunset data.");
        }
            
        var response = new SunriseSunsetResult
        {
            Sunrise = result.Results.Sunrise,
            Sunset = result.Results.Sunset
        };
            
        return Ok(response);
    }
        
    private async Task<GeocodingResponse?> GetCoordinatesAsync(string city)
    {
        var client = _httpClientFactory.CreateClient("GeocodingClient");
            
        var apiKey = _configuration["GeocodingAPI:ApiKey"];
            
        var url = $"{_configuration["GeocodingAPI:BaseUrl"]}?q={city}&limit=1&appid={apiKey}";
            
        var response = await client.GetAsync(url);
            
        if (!response.IsSuccessStatusCode) return null;
            
        var data = await response.Content.ReadFromJsonAsync<List<GeocodingResponse>>();
            
        return data?.FirstOrDefault();
    }
        
    private async Task<SunriseSunsetApiResponse?> GetSunriseSunsetTimesAsync(double latitude, double longitude, string date)
    {
        var client = _httpClientFactory.CreateClient("SunsetClient");
            
        var url = $"{_configuration["SunriseSunsetAPI:BaseUrl"]}?lat={latitude}&lng={longitude}&date={date}&formatted=0";
            
        var response = await client.GetAsync(url);
            
        if (!response.IsSuccessStatusCode) return null;
            
        return await response.Content.ReadFromJsonAsync<SunriseSunsetApiResponse>();
    }
}