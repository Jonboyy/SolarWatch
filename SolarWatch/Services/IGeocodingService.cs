using SolarWatch.Models;

namespace SolarWatch.Services;

public interface IGeocodingService
{
    Task<GeocodingData> GetCoordinatesAsync(string city);
}