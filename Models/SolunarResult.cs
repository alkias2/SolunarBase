namespace SolunarBase.Models;

public class MoonPhaseInfo
{
    public string Phase { get; set; } = string.Empty;
    public double Illumination { get; set; }
}

/// <summary>
/// Detailed breakdown of activity score components for a specific hour.
/// </summary>
public class ActivityBreakdown
{
    /// <summary>
    /// The hour of the day (0-23).
    /// </summary>
    public int Hour { get; set; }

    /// <summary>
    /// Base solunar score (from major/minor periods, moon phase, time of day).
    /// </summary>
    public double SolunarScore { get; set; }

    /// <summary>
    /// Weather modifier contribution.
    /// </summary>
    public double WeatherModifier { get; set; }

    /// <summary>
    /// Tide modifier contribution.
    /// </summary>
    public double TideModifier { get; set; }

    /// <summary>
    /// Final combined score (0-100).
    /// </summary>
    public int TotalScore { get; set; }
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
    
    /// <summary>
    /// Detailed astronomical data for the Sun and Moon.
    /// </summary>
    public AstronomicalData? Astronomy { get; set; }

    /// <summary>
    /// Optional detailed breakdown of how each hour's score was calculated.
    /// Only populated when weather and/or tide data is provided.
    /// </summary>
    public List<ActivityBreakdown>? ActivityBreakdown { get; set; }

    /// <summary>
    /// Indicates whether weather modifiers were applied.
    /// </summary>
    public bool HasWeatherModifiers { get; set; }

    /// <summary>
    /// Indicates whether tide modifiers were applied.
    /// </summary>
    public bool HasTideModifiers { get; set; }
}

public class Location
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
