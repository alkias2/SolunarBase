namespace SolunarBase.Models;

/// <summary>
/// Input parameters for solunar calculation.
/// </summary>
public class SolunarInput
{
    /// <summary>
    /// Geographic latitude in degrees (-90 to 90).
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Geographic longitude in degrees (-180 to 180).
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// The date for which to calculate solunar activity.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Time zone identifier (e.g., "Europe/Athens", "America/New_York").
    /// If null, system default will be used.
    /// </summary>
    public string? TimeZoneId { get; set; }

    /// <summary>
    /// Optional weather data for the day (hourly observations).
    /// When provided, weather modifiers will be applied to activity scores.
    /// </summary>
    public List<WeatherData>? WeatherData { get; set; }

    /// <summary>
    /// Optional tide data for the day (high/low tide events).
    /// When provided, tide modifiers will be applied to activity scores.
    /// </summary>
    public List<TideData>? TideData { get; set; }

    /// <summary>
    /// Optional custom weights for solunar, weather, and tide factors.
    /// If null, default weights will be used.
    /// </summary>
    public ModifierWeights? Weights { get; set; }
}
