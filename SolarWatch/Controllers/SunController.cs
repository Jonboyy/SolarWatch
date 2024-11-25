using Microsoft.AspNetCore.Mvc;
using SolarWatch.Models;
using SolarWatch.Services;
using System;
using System.Threading.Tasks;

namespace SolarWatch.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SunController : ControllerBase
    {
        private readonly IGeocodingService _geocodingService;
        private readonly ISunriseSunsetService _sunriseSunsetService;
        private readonly ITimeZoneService _timeZoneService;

        public SunController(IGeocodingService geocodingService, ISunriseSunsetService sunriseSunsetService, ITimeZoneService timeZoneService)
        {
            _geocodingService = geocodingService;
            _sunriseSunsetService = sunriseSunsetService;
            _timeZoneService = timeZoneService;
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

                var targetDate = request.Date == default ? DateTime.UtcNow.Date : request.Date.Date;

                var timeZone = await _timeZoneService.GetTimeZoneAsync(geocodingData.Latitude, geocodingData.Longitude);

                var sunTimes = await _sunriseSunsetService.GetSunTimesAsync(geocodingData.Latitude, geocodingData.Longitude, targetDate, timeZone);

                var response = new SunTimesResponse
                {
                    City = request.City,
                    Date = targetDate.ToString("yyyy-MM-dd"),
                    Timezone = request.Timezone,
                    Sunrise = request.Timezone.Equals("utc", StringComparison.OrdinalIgnoreCase) ? sunTimes.SunriseUtc : sunTimes.SunriseLocal,
                    Sunset = request.Timezone.Equals("utc", StringComparison.OrdinalIgnoreCase) ? sunTimes.SunsetUtc : sunTimes.SunsetLocal
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





