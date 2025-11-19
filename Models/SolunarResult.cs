namespace SolunarBase.Models;

public class MoonPhaseInfo
{
    public string Phase { get; set; } = string.Empty;
    public double Illumination { get; set; }
}

public class SolunarResult
{
    public DateOnly Date { get; set; }
    public Location Location { get; set; } = new();
    public string TimeZoneId { get; set; } = "UTC";
    public List<SolunarPeriod> MajorTimes { get; set; } = new();
    public List<SolunarPeriod> MinorTimes { get; set; } = new();
    public List<HourlyActivity> HourlyActivity { get; set; } = new();
    public MoonPhaseInfo MoonPhase { get; set; } = new();
    public string SolunarRating { get; set; } = string.Empty;
}

public class Location
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
