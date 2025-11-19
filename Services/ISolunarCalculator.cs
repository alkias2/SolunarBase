using SolunarBase.Models;

namespace SolunarBase.Services;

/// <summary>
/// Defines the contract for solunar theory calculations to determine optimal fishing and hunting times.
/// </summary>
public interface ISolunarCalculator
{
    /// <summary>
    /// Calculates the complete solunar forecast for a given date and location.
    /// </summary>
    /// <param name="input">The input parameters including date, location (latitude/longitude), and time zone.</param>
    /// <returns>
    /// A SolunarResult object containing:
    /// - Major activity periods (2-hour windows centered on lunar transits)
    /// - Minor activity periods (1-hour windows centered on moonrise/moonset)
    /// - Hourly activity scores (0-100) for each hour of the day
    /// - Overall daily solunar rating (Excellent/Good/Fair/Poor)
    /// - Moon phase information
    /// </returns>
    /// <remarks>
    /// The calculation process:
    /// 1. Retrieves astronomical data (sun/moon rise/set times, lunar transits, moon phase)
    /// 2. Creates major periods (±1 hour around upper/lower lunar transits)
    /// 3. Creates minor periods (1 hour starting from moonrise/moonset)
    /// 4. Calculates hourly scores based on proximity to period centers using Gaussian decay
    /// 5. Applies multipliers for moon phase (0.9-1.1) and time of day (0.8-1.1)
    /// 6. Adds overlap bonuses when major and minor periods coincide
    /// 7. Converts all times from UTC to local time zone for output
    /// </remarks>
    SolunarResult Calculate(SolunarInput input);
}
