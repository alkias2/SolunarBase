namespace SolunarBase.Models;

/// <summary>
/// Sun-related astronomical data for a specific date and location.
/// </summary>
public class SunData
{
    /// <summary>
    /// Sunrise time in UTC (null if no sunrise on this day).
    /// </summary>
    public DateTime? RiseUtc { get; set; }

    /// <summary>
    /// Sunrise time in local time zone (null if no sunrise on this day).
    /// </summary>
    public DateTime? RiseLocal { get; set; }

    /// <summary>
    /// Sunset time in UTC (null if no sunset on this day).
    /// </summary>
    public DateTime? SetUtc { get; set; }

    /// <summary>
    /// Sunset time in local time zone (null if no sunset on this day).
    /// </summary>
    public DateTime? SetLocal { get; set; }

    /// <summary>
    /// Time when the Sun reaches its highest point (culmination) in UTC.
    /// </summary>
    public DateTime? CulminationUtc { get; set; }

    /// <summary>
    /// Time when the Sun reaches its highest point (culmination) in local time zone.
    /// </summary>
    public DateTime? CulminationLocal { get; set; }

    /// <summary>
    /// Altitude angle in degrees at culmination.
    /// </summary>
    public double? CulminationAltitudeDegrees { get; set; }

    /// <summary>
    /// Azimuth angle in degrees at culmination (typically near 180 for northern hemisphere).
    /// </summary>
    public double? CulminationAzimuthDegrees { get; set; }
}

/// <summary>
/// Moon-related astronomical data for a specific date and location.
/// </summary>
public class MoonData
{
    /// <summary>
    /// Moonrise time in UTC (null if no moonrise on this day).
    /// </summary>
    public DateTime? RiseUtc { get; set; }

    /// <summary>
    /// Moonrise time in local time zone (null if no moonrise on this day).
    /// </summary>
    public DateTime? RiseLocal { get; set; }

    /// <summary>
    /// Moonset time in UTC (null if no moonset on this day).
    /// </summary>
    public DateTime? SetUtc { get; set; }

    /// <summary>
    /// Moonset time in local time zone (null if no moonset on this day).
    /// </summary>
    public DateTime? SetLocal { get; set; }

    /// <summary>
    /// Time of upper transit (culmination) - when Moon is highest in the sky, in UTC.
    /// </summary>
    public DateTime? UpperTransitUtc { get; set; }

    /// <summary>
    /// Time of upper transit (culmination) in local time zone.
    /// </summary>
    public DateTime? UpperTransitLocal { get; set; }

    /// <summary>
    /// Altitude angle in degrees at upper transit.
    /// </summary>
    public double? UpperTransitAltitudeDegrees { get; set; }

    /// <summary>
    /// Azimuth angle in degrees at upper transit (typically near 180 for northern hemisphere).
    /// </summary>
    public double? UpperTransitAzimuthDegrees { get; set; }

    /// <summary>
    /// Time of lower transit - when Moon is at its lowest point, in UTC.
    /// </summary>
    public DateTime? LowerTransitUtc { get; set; }

    /// <summary>
    /// Time of lower transit in local time zone.
    /// </summary>
    public DateTime? LowerTransitLocal { get; set; }

    /// <summary>
    /// Altitude angle in degrees at lower transit (typically negative, below horizon).
    /// </summary>
    public double? LowerTransitAltitudeDegrees { get; set; }

    /// <summary>
    /// Phase of the Moon as a name (e.g., "New Moon", "Full Moon", "Waxing Crescent").
    /// </summary>
    public string Phase { get; set; } = string.Empty;

    /// <summary>
    /// Illumination fraction of the Moon (0.0 = new moon, 1.0 = full moon).
    /// </summary>
    public double Illumination { get; set; }

    /// <summary>
    /// Phase angle in degrees (0 = new, 90 = first quarter, 180 = full, 270 = third quarter).
    /// </summary>
    public double PhaseAngle { get; set; }

    /// <summary>
    /// Distance from Earth to Moon in kilometers.
    /// </summary>
    public double? DistanceKm { get; set; }
}

/// <summary>
/// Comprehensive astronomical data for the Sun and Moon for a specific date and location.
/// </summary>
public class AstronomicalData
{
    /// <summary>
    /// Sun-related data.
    /// </summary>
    public SunData Sun { get; set; } = new();

    /// <summary>
    /// Moon-related data.
    /// </summary>
    public MoonData Moon { get; set; } = new();
}
