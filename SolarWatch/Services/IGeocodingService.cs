namespace SolarWatch.Services;

public interface IGeocodingService
{
    (double Latitude, double Longitude) GetCoordinates(string city);
}