using SolarWatch.Models;
using System;
using System.Threading.Tasks;

namespace SolarWatch.Services
{
    public interface ISunriseSunsetService
    {
        Task<SunTimes> GetSunTimesAsync(double latitude, double longitude, DateTime date, TimeZoneInfo timeZone);
    }
}