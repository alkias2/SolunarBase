using SolunarBase.Models;

namespace SolunarBase.Services;

/// <summary>
/// Defines the contract for astronomical calculations related to sun and moon positions and events.
/// Uses the CosineKitty.AstronomyEngine library for precise calculations.
/// </summary>
public interface IAstronomyCalculator
{
    /// <summary>
    /// Calculates comprehensive astronomical data for the Sun and Moon for a specific location and date.
    /// </summary>
    /// <param name="latitude">The geographic latitude of the location in degrees (-90 to 90).</param>
    /// <param name="longitude">The geographic longitude of the location in degrees (-180 to 180).</param>
    /// <param name="date">The date for which to calculate astronomical data.</param>
    /// <param name="timeZoneId">The time zone ID for the location (e.g., "Europe/Athens").</param>
    /// <returns>
    /// An AstronomicalData object containing detailed information about:
    /// - Sun: rise, set, culmination times and positions
    /// - Moon: rise, set, transit times, phase, illumination, and distance
    /// </returns>
    /// <remarks>
    /// Uses the CosineKitty.AstronomyEngine library for high-precision calculations.
    /// All times are returned in UTC and should be converted to local time as needed.
    /// </remarks>
    AstronomicalData GetAstronomicalData(double latitude, double longitude, DateOnly date, string timeZoneId);
}
