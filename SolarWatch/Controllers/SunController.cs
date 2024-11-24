using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using SolarWatch.Services;

namespace SolarWatch.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SunController : ControllerBase
{
    private readonly IGeocodingService _geocodingService;
    private readonly ISunriseSunsetService _sunriseSunsetService;

    public SunController(IGeocodingService geocodingService, ISunriseSunsetService sunriseSunsetService)
    {
        _geocodingService = geocodingService;
        _sunriseSunsetService = sunriseSunsetService;
    }

    [HttpGet]
    public IActionResult GetSunTimes([FromQuery] string city, [FromQuery] string date = null, [FromQuery] string timezone = "local")
    {
        if (string.IsNullOrEmpty(city))
        {
            return BadRequest(new
            {
                error = "Missing parameter",
                details = "The 'city' parameter is required."
            });
        }

        try
        {
            var (latitude, longitude) = _geocodingService.GetCoordinates(city);
            var targetDate = date ?? DateTime.UtcNow.ToString("yyyy-MM-dd");

            if (!DateTime.TryParse(targetDate, out DateTime parsedDate))
            {
                return BadRequest(new
                {
                    error = "Invalid date format",
                    details = "Please provide the date in YYYY-MM-DD format."
                });
            }

            var sunTimes = _sunriseSunsetService.GetSunTimes(latitude, longitude, parsedDate);
            return Ok(new
            {
                city,
                date = targetDate,
                timezone,
                sunrise = timezone == "utc" ? sunTimes.SunriseUtc : sunTimes.SunriseLocal,
                sunset = timezone == "utc" ? sunTimes.SunsetUtc : sunTimes.SunsetLocal
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                error = "Internal Server Error",
                details = ex.Message
            });
        }
    }
}

