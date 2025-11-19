namespace SolunarBase.Models;

public class SolunarInput
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateOnly Date { get; set; }
    public string? TimeZoneId { get; set; }
}
