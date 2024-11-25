using Microsoft.AspNetCore.Mvc;
using SolarWatch.Models;
using SolarWatch.Services;

namespace SolarWatch.Controllers
{
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
        public async Task<IActionResult> GetSunTimes([FromQuery] SunTimesRequest request)
        {
            if (string.IsNullOrEmpty(request.City))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Missing parameter",
                    Details = "The 'city' parameter is required."
                });
            }

            try
            {
                var geocodingData = await _geocodingService.GetCoordinatesAsync(request.City);

                var targetDate = request.Date == default ? DateTime.UtcNow : request.Date;

                var sunTimes = await _sunriseSunsetService.GetSunTimesAsync(geocodingData.Latitude, geocodingData.Longitude, targetDate);

                var response = new SunTimesResponse
                {
                    City = request.City,
                    Date = targetDate.ToString("yyyy-MM-dd"),
                    Timezone = request.Timezone,
                    Sunrise = request.Timezone.ToLower() == "utc" ? sunTimes.SunriseUtc : sunTimes.SunriseLocal,
                    Sunset = request.Timezone.ToLower() == "utc" ? sunTimes.SunsetUtc : sunTimes.SunsetLocal
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Error = "Internal Server Error",
                    Details = ex.Message
                });
            }
        }
    }
}



