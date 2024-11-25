using System.Threading.Tasks;

namespace SolarWatch.Services
{
    public interface ITimeZoneService
    {
        Task<TimeZoneInfo> GetTimeZoneAsync(double latitude, double longitude);
    }
}