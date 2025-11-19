using SolunarBase.Models;

namespace SolunarBase.Services;

/// <summary>
/// Defines the contract for astronomical calculations related to sun and moon positions and events.
/// </summary>
public interface IAstronomyCalculator
{
    /// <summary>
    /// Calculates the sunrise and sunset times for a specific location and date.
    /// </summary>
    /// <param name="latitude">The geographic latitude of the location in degrees (-90 to 90).</param>
    /// <param name="longitude">The geographic longitude of the location in degrees (-180 to 180).</param>
    /// <param name="date">The date for which to calculate the sunrise/sunset times.</param>
    /// <returns>
    /// A tuple containing:
    /// - SunriseUtc: The sunrise time in UTC (null if there is no sunrise on that day).
    /// - SunsetUtc: The sunset time in UTC (null if there is no sunset on that day).
    /// </returns>
    /// <remarks>
    /// Uses the CoordinateSharp library for astronomical calculations.
    /// At extreme latitudes (near the poles), there may be no sunrise or sunset.
    /// </remarks>
    (DateTime? SunriseUtc, DateTime? SunsetUtc) GetSunTimes(double latitude, double longitude, DateOnly date);
    
    /// <summary>
    /// Calculates the moonrise and moonset times for a specific location and date.
    /// </summary>
    /// <param name="latitude">The geographic latitude of the location in degrees (-90 to 90).</param>
    /// <param name="longitude">The geographic longitude of the location in degrees (-180 to 180).</param>
    /// <param name="date">The date for which to calculate the moonrise/moonset times.</param>
    /// <returns>
    /// A tuple containing:
    /// - MoonriseUtc: The moonrise time in UTC (null if there is no moonrise).
    /// - MoonsetUtc: The moonset time in UTC (null if there is no moonset).
    /// </returns>
    /// <remarks>
    /// The moon may not have a rise or set on certain days, depending on its phase and the latitude.
    /// </remarks>
    (DateTime? MoonriseUtc, DateTime? MoonsetUtc) GetMoonTimes(double latitude, double longitude, DateOnly date);
    
    /// <summary>
    /// Calculates the lunar transit times (upper and lower transit) for a specific location and date.
    /// </summary>
    /// <param name="latitude">The geographic latitude of the location in degrees (-90 to 90).</param>
    /// <param name="longitude">The geographic longitude of the location in degrees (-180 to 180).</param>
    /// <param name="date">The date for which to calculate the transits.</param>
    /// <returns>
    /// A tuple containing:
    /// - UpperTransitUtc: The time of upper transit (when the moon is at its highest point) in UTC.
    /// - LowerTransitUtc: The time of lower transit (when the moon is at its lowest point) in UTC.
    /// </returns>
    /// <remarks>
    /// The method uses a two-phase approach:
    /// 1. Coarse search with 5-minute steps to find the region of the extrema.
    /// 2. Fine-grained search with 1-minute steps in a ±30 minute window around the extrema.
    /// Upper transit occurs when the moon crosses the meridian (usually southward).
    /// Lower transit occurs on the opposite side (usually northward, below the horizon).
    /// </remarks>
    (DateTime? UpperTransitUtc, DateTime? LowerTransitUtc) GetLunarTransits(double latitude, double longitude, DateOnly date);
    
    /// <summary>
    /// Calculates the moon phase and illumination for a specific location and date.
    /// </summary>
    /// <param name="latitude">The geographic latitude of the location in degrees (-90 to 90).</param>
    /// <param name="longitude">The geographic longitude of the location in degrees (-180 to 180).</param>
    /// <param name="date">The date for which to calculate the moon phase.</param>
    /// <returns>
    /// A MoonPhaseInfo object containing:
    /// - Phase: The name of the phase (e.g., "New Moon", "Full Moon", "First Quarter", etc.).
    /// - Illumination: The illumination percentage of the moon (0.0 = new moon, 1.0 = full moon).
    /// </returns>
    /// <remarks>
    /// The calculation is performed at 12:00 UTC to get a representative value for the day.
    /// The illumination percentage is clamped to the range [0, 1] for safety.
    /// </remarks>
    MoonPhaseInfo GetMoonPhase(double latitude, double longitude, DateOnly date);
}
