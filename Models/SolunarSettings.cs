namespace SolunarBase.Models;

public class SolunarSettings
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string TimeZoneId { get; set; } = "Europe/Athens";
    public string OutputDirectory { get; set; } = "Output";
}
