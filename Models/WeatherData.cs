namespace SolunarBase.Models;

/// <summary>
/// Represents hourly weather data that affects fish activity.
/// </summary>
/// <remarks>
/// Weather conditions act as modifiers to the base solunar activity scores.
/// Key factors include water temperature (most critical), air pressure, wind, and wave conditions.
/// </remarks>
public class WeatherData
{
    /// <summary>
    /// Unique identifier for this weather record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The timestamp for this weather observation (typically in UTC or local time).
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Air temperature in degrees Celsius.
    /// </summary>
    /// <remarks>
    /// Extreme temperatures can reduce fish activity. Stable temperatures are preferred.
    /// Modifier range: -5 to +5
    /// </remarks>
    public double AirTemperature { get; set; }

    /// <summary>
    /// Water temperature in degrees Celsius.
    /// </summary>
    /// <remarks>
    /// This is the MOST IMPORTANT weather factor for fish activity.
    /// Different species have optimal temperature ranges.
    /// Modifier range: -15 to +15
    /// </remarks>
    public double WaterTemperature { get; set; }

    /// <summary>
    /// Cloud cover percentage (0-100).
    /// </summary>
    /// <remarks>
    /// Moderate cloud cover often increases fish activity. Clear skies can reduce it.
    /// Modifier range: -5 to +10
    /// </remarks>
    public double CloudCover { get; set; }

    /// <summary>
    /// Wind direction in degrees (0-360).
    /// </summary>
    public double WindDirection { get; set; }

    /// <summary>
    /// Wind speed in meters per second.
    /// </summary>
    /// <remarks>
    /// Moderate wind can increase activity. Too strong wind reduces it.
    /// Combined with direction for modifier range: -10 to +10
    /// </remarks>
    public double WindSpeed { get; set; }

    /// <summary>
    /// Wind gust speed in meters per second.
    /// </summary>
    public double WindGust { get; set; }

    /// <summary>
    /// Atmospheric pressure in millibars (hPa).
    /// </summary>
    /// <remarks>
    /// Rising pressure: positive. Falling pressure: can be positive or negative.
    /// Stable high pressure: best. Rapid drops: worst.
    /// Modifier range: -15 to +15
    /// </remarks>
    public double Pressure { get; set; }

    /// <summary>
    /// Relative humidity percentage (0-100).
    /// </summary>
    /// <remarks>
    /// Minor effect on fish activity.
    /// Modifier range: -3 to +3
    /// </remarks>
    public double Humidity { get; set; }

    /// <summary>
    /// Visibility in kilometers.
    /// </summary>
    public double Visibility { get; set; }

    /// <summary>
    /// Wave height in meters.
    /// </summary>
    /// <remarks>
    /// Part of wave/swell/current system.
    /// Moderate waves: positive. Very high waves: negative.
    /// </remarks>
    public double WaveHeight { get; set; }

    /// <summary>
    /// Wave direction in degrees (0-360).
    /// </summary>
    public double WaveDirection { get; set; }

    /// <summary>
    /// Wave period in seconds.
    /// </summary>
    public double WavePeriod { get; set; }

    /// <summary>
    /// Swell height in meters.
    /// </summary>
    public double SwellHeight { get; set; }

    /// <summary>
    /// Swell direction in degrees (0-360).
    /// </summary>
    public double SwellDirection { get; set; }

    /// <summary>
    /// Swell period in seconds.
    /// </summary>
    public double SwellPeriod { get; set; }

    /// <summary>
    /// Current direction in degrees (0-360).
    /// </summary>
    public double CurrentDirection { get; set; }

    /// <summary>
    /// Current speed in meters per second.
    /// </summary>
    /// <remarks>
    /// Moderate current increases feeding activity.
    /// Combined wave/swell/current modifier range: -10 to +10
    /// </remarks>
    public double CurrentSpeed { get; set; }

    /// <summary>
    /// Data source identifier (e.g., "sg" for Stormglass).
    /// </summary>
    public string DataSource { get; set; } = string.Empty;
}

/// <summary>
/// Container for a collection of weather data records.
/// </summary>
public class WeatherDataCollection
{
    /// <summary>
    /// List of hourly weather observations.
    /// </summary>
    public List<WeatherData> Weather { get; set; } = new();
}
