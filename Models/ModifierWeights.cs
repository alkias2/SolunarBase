namespace SolunarBase.Models;

/// <summary>
/// Configuration for weighting different factors in the solunar activity calculation.
/// </summary>
/// <remarks>
/// This allows fine-tuning the influence of solunar, weather, and tide factors.
/// Weights typically range from 0.0 (no influence) to 1.0 (full influence).
/// Values can exceed 1.0 for emphasis on particularly important factors.
/// </remarks>
public class ModifierWeights
{
    /// <summary>
    /// Weights for solunar theory components.
    /// </summary>
    public SolunarWeights Solunar { get; set; } = new();

    /// <summary>
    /// Weights for weather-based modifiers.
    /// </summary>
    public WeatherWeights Weather { get; set; } = new();

    /// <summary>
    /// Weights for tide-based modifiers.
    /// </summary>
    public TideWeights Tide { get; set; } = new();
}

/// <summary>
/// Weights for solunar theory components.
/// </summary>
public class SolunarWeights
{
    /// <summary>
    /// Weight for major periods (lunar transits).
    /// Default: 1.0 (full strength)
    /// </summary>
    public double Major { get; set; } = 1.0;

    /// <summary>
    /// Weight for minor periods (moonrise/moonset).
    /// Default: 0.6 (60% of major period strength)
    /// </summary>
    public double Minor { get; set; } = 0.6;

    /// <summary>
    /// Weight for moon phase multiplier effect.
    /// Default: 0.3 (30% influence)
    /// </summary>
    public double MoonPhase { get; set; } = 0.3;
}

/// <summary>
/// Weights for weather-based modifiers.
/// </summary>
public class WeatherWeights
{
    /// <summary>
    /// Weight for water temperature effects.
    /// Default: 0.9 (critical factor)
    /// </summary>
    public double WaterTemperature { get; set; } = 0.9;

    /// <summary>
    /// Weight for atmospheric pressure effects.
    /// Default: 0.8 (very important)
    /// </summary>
    public double Pressure { get; set; } = 0.8;

    /// <summary>
    /// Weight for wind effects.
    /// Default: 0.7 (important)
    /// </summary>
    public double Wind { get; set; } = 0.7;

    /// <summary>
    /// Weight for cloud cover effects.
    /// Default: 0.5 (moderate importance)
    /// </summary>
    public double CloudCover { get; set; } = 0.5;

    /// <summary>
    /// Weight for wave/swell/current effects.
    /// Default: 0.6 (moderate to high importance)
    /// </summary>
    public double Waves { get; set; } = 0.6;

    /// <summary>
    /// Weight for air temperature effects.
    /// Default: 0.4 (less critical than water temp)
    /// </summary>
    public double AirTemperature { get; set; } = 0.4;

    /// <summary>
    /// Weight for humidity effects.
    /// Default: 0.2 (minor factor)
    /// </summary>
    public double Humidity { get; set; } = 0.2;
}

/// <summary>
/// Weights for tide-based modifiers.
/// </summary>
public class TideWeights
{
    /// <summary>
    /// Weight for tide level (high vs low) effects.
    /// Default: 0.8 (important)
    /// </summary>
    public double Level { get; set; } = 0.8;

    /// <summary>
    /// Weight for tidal movement (current strength) effects.
    /// Default: 1.0 (critical - moving water is key)
    /// </summary>
    public double Movement { get; set; } = 1.0;
}
