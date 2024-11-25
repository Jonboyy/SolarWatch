using SolarWatch.Models;

namespace SolarWatch.Services;

public interface ISunriseSunsetService
{
    Task<SunTimes> GetSunTimesAsync(double latitude, double longitude, DateTime date);
}