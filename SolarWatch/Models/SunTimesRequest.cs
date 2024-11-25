namespace SolarWatch.Models;

public class SunTimesRequest
{
    public string City { get; set; }
    public DateTime Date { get; set; }
    public string Timezone { get; set; } = "local";
}