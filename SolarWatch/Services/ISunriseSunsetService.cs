namespace SolarWatch.Services;

public interface ISunriseSunsetService
{
    (string SunriseUtc, string SunsetUtc, string SunriseLocal, string SunsetLocal) GetSunTimes(double latitude, double longitude, DateTime date);
}