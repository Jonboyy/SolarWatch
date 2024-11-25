using SolarWatch.Models;

namespace SolarWatch.Services;

public interface ISunriseSunsetService
{
    SunTimes GetSunTimes(double latitude, double longitude, DateTime date);
}