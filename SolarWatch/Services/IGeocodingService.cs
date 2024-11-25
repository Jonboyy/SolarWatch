using SolarWatch.Models;

namespace SolarWatch.Services;

public interface IGeocodingService
{
    GeocodingData GetCoordinates(string city);
}