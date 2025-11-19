using CoordinateSharp;
using SolunarBase.Models;

namespace SolunarBase.Services;

public class AstronomyCalculator : IAstronomyCalculator
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
    public (DateTime? SunriseUtc, DateTime? SunsetUtc) GetSunTimes(double latitude, double longitude, DateOnly date)
    {
        // Create a Celestial object with the coordinates and date
        // We use TimeOnly.MinValue (00:00) to start from the beginning of the day
        var c = new Celestial(latitude, longitude, date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        
        // CoordinateSharp automatically calculates the sunrise/sunset times
        // Return the results as a tuple
        return (c.SunRise, c.SunSet);
    }

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
    public (DateTime? MoonriseUtc, DateTime? MoonsetUtc) GetMoonTimes(double latitude, double longitude, DateOnly date)
    {
        // Create a Celestial object with the coordinates and date
        // Just like with the sun, we start from the beginning of the day (00:00 UTC)
        var c = new Celestial(latitude, longitude, date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        
        // CoordinateSharp automatically calculates the moonrise/moonset times
        // Return the results as a tuple
        return (c.MoonRise, c.MoonSet);
    }

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
    public (DateTime? UpperTransitUtc, DateTime? LowerTransitUtc) GetLunarTransits(double latitude, double longitude, DateOnly date)
    {
        // PHASE 1: Coarse search to find the region of the extrema
        // Define the time window: the entire day from 00:00 to 24:00 UTC
        DateTime start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        DateTime end = start.AddDays(1);
        
        // Use 5-minute steps for a quick scan of the entire day
        // This gives us 288 samples per day (24 hours * 12 samples/hour)
        TimeSpan coarseStep = TimeSpan.FromMinutes(5);

        // Initialize variables to track the extrema
        DateTime? maxT = null, minT = null;  // The times of the extrema
        double maxAlt = double.NegativeInfinity;  // The maximum altitude angle (upper transit)
        double minAlt = double.PositiveInfinity;  // The minimum altitude angle (lower transit)

        // Scan the entire day with 5-minute steps
        for (var t = start; t < end; t += coarseStep)
        {
            // For each time instant, calculate the moon's position
            var cel = new Celestial(latitude, longitude, t);
            double alt = cel.MoonAltitude;  // Moon's altitude angle in degrees
            
            // Check if we found a new maximum (upper transit)
            if (alt > maxAlt)
            {
                maxAlt = alt;
                maxT = t;
            }
            
            // Check if we found a new minimum (lower transit)
            if (alt < minAlt)
            {
                minAlt = alt;
                minT = t;
            }
        }

        // PHASE 2: Fine-grained search around the extrema
        // Improve accuracy with 1-minute steps in a ±30 minute window
        // seekMax=true for upper transit, false for lower transit
        DateTime? refineMax = RefineExtremum(latitude, longitude, maxT, true);
        DateTime? refineMin = RefineExtremum(latitude, longitude, minT, false);

        // Return the refined results
        return (refineMax, refineMin);
    }

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
    public MoonPhaseInfo GetMoonPhase(double latitude, double longitude, DateOnly date)
    {
        // Calculate the moon phase at 12:00 UTC (noon)
        // We choose noon to have a representative value for the day
        var cel = new Celestial(latitude, longitude, date.ToDateTime(new TimeOnly(12,0), DateTimeKind.Utc));
        
        // Get the moon illumination information from CoordinateSharp
        var illum = cel.MoonIllum;
        
        // Create and return the MoonPhaseInfo object
        return new MoonPhaseInfo
        {
            // The phase name (e.g., "Waxing Crescent", "Full Moon", etc.)
            Phase = illum.PhaseName,
            
            // The illumination percentage (0-1), we use Clamp for safety
            // to ensure the value is always within [0, 1]
            Illumination = Math.Clamp(illum.Fraction, 0, 1)
        };
    }

    /// <summary>
    /// Refines the accuracy of finding an extremum (maximum or minimum) of the moon's altitude angle.
    /// </summary>
    /// <param name="latitude">The geographic latitude of the location in degrees (-90 to 90).</param>
    /// <param name="longitude">The geographic longitude of the location in degrees (-180 to 180).</param>
    /// <param name="seed">The preliminary time around which the detailed search will be performed.</param>
    /// <param name="seekMax">True to search for maximum (upper transit), false for minimum (lower transit).</param>
    /// <returns>
    /// The time of the extremum in UTC with 1-minute accuracy, or null if no seed was provided.
    /// </returns>
    /// <remarks>
    /// The method examines a ±30 minute window around the seed with 1-minute steps (61 samples).
    /// This improves accuracy from the coarse 5-minute step to the final 1-minute step.
    /// </remarks>
    private static DateTime? RefineExtremum(double latitude, double longitude, DateTime? seed, bool seekMax)
    {
        // If no seed was provided (no extremum found in coarse search), return null
        if (seed == null) return null;
        
        // Define the time window: ±30 minutes around the seed
        // This gives 1 hour total for detailed scanning
        var windowStart = seed.Value.AddMinutes(-30);
        var windowEnd = seed.Value.AddMinutes(30);
        
        // Use 1-minute steps for high accuracy
        // This gives us 61 samples (from -30 to +30 minutes)
        var step = TimeSpan.FromMinutes(1);
        
        // Initialize variables to track the best extremum
        DateTime? bestT = null;  // The time of the best extremum
        
        // Initialize bestAlt depending on whether we're searching for maximum or minimum
        double bestAlt = seekMax ? double.NegativeInfinity : double.PositiveInfinity;

        // Scan the window with 1-minute steps
        for (var t = windowStart; t <= windowEnd; t += step)
        {
            // Calculate the moon's altitude angle for this time instant
            var cel = new Celestial(latitude, longitude, t);
            double alt = cel.MoonAltitude;
            
            // If searching for maximum (upper transit)
            if (seekMax)
            {
                // Check if we found a higher altitude angle
                if (alt > bestAlt)
                {
                    bestAlt = alt;  // Update the maximum angle
                    bestT = t;       // Store the time instant
                }
            }
            else  // If searching for minimum (lower transit)
            {
                // Check if we found a lower altitude angle
                if (alt < bestAlt)
                {
                    bestAlt = alt;  // Update the minimum angle
                    bestT = t;       // Store the time instant
                }
            }
        }
        
        // Return the time of the extremum with 1-minute accuracy
        return bestT;
    }
}
