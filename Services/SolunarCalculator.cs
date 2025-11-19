using SolunarBase.Models;
using SolunarBase.Utils;

namespace SolunarBase.Services;

/// <summary>
/// Implements solunar theory calculations to determine optimal fishing and hunting times based on lunar and solar positions.
/// </summary>
/// <remarks>
/// The Solunar Theory, developed by John Alden Knight, suggests that fish and game are more active during certain periods
/// related to the moon's position. This calculator computes major and minor activity periods, hourly activity scores,
/// and overall daily ratings based on lunar transits, moonrise/moonset, moon phase, and time of day.
/// </remarks>
public class SolunarCalculator : ISolunarCalculator
{
    private readonly IAstronomyCalculator _astro;

    /// <summary>
    /// Initializes a new instance of the SolunarCalculator class.
    /// </summary>
    /// <param name="astro">The astronomy calculator used to obtain celestial events (sun/moon positions and times).</param>
    public SolunarCalculator(IAstronomyCalculator astro)
    {
        _astro = astro;
    }

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
    public SolunarResult Calculate(SolunarInput input)
    {
        // Resolve the time zone from the input (e.g., "Europe/Athens" or system default)
        // This ensures all local time conversions use the correct time zone
        var tz = TimeZoneHelper.Resolve(input.TimeZoneId);
        
        // Initialize the result object with basic information
        var result = new SolunarResult
        {
            Date = input.Date,
            Location = new Location { Latitude = input.Latitude, Longitude = input.Longitude },
            TimeZoneId = tz.Id
        };

        // STEP 1: Retrieve all astronomical data for the given location and date
        // All times returned are in UTC for consistent calculations
        var (sunrise, sunset) = _astro.GetSunTimes(input.Latitude, input.Longitude, input.Date);
        var (moonrise, moonset) = _astro.GetMoonTimes(input.Latitude, input.Longitude, input.Date);
        var (upper, lower) = _astro.GetLunarTransits(input.Latitude, input.Longitude, input.Date);
        result.MoonPhase = _astro.GetMoonPhase(input.Latitude, input.Longitude, input.Date);

        // STEP 2: Build UTC periods for major and minor activity times
        // Major periods are 2-hour windows (±1 hour) centered on lunar transits
        // Minor periods are 1-hour windows starting from moonrise/moonset
        var majorUtc = new List<SolunarPeriod>();
        var minorUtc = new List<SolunarPeriod>();
        
        // Major period: Upper transit (moon at highest point in the sky)
        // This is considered the most active feeding/hunting time
        if (upper.HasValue)
        {
            var start = upper.Value.AddHours(-1);  // 1 hour before transit
            var end = upper.Value.AddHours(1);      // 1 hour after transit
            majorUtc.Add(new SolunarPeriod { Type = SolunarPeriodType.UpperTransit, Start = start, End = end, Center = upper.Value });
        }
        
        // Major period: Lower transit (moon at lowest point, often below horizon)
        // Also considered a highly active period
        if (lower.HasValue)
        {
            var start = lower.Value.AddHours(-1);
            var end = lower.Value.AddHours(1);
            majorUtc.Add(new SolunarPeriod { Type = SolunarPeriodType.LowerTransit, Start = start, End = end, Center = lower.Value });
        }
        
        // Minor period: Moonrise (moon appears on the horizon)
        // Activity period is 1 hour starting from moonrise, centered at +30 minutes
        if (moonrise.HasValue)
        {
            var center = moonrise.Value.AddMinutes(30);  // Center is 30 minutes after moonrise
            minorUtc.Add(new SolunarPeriod { Type = SolunarPeriodType.Moonrise, Start = moonrise.Value, End = moonrise.Value.AddHours(1), Center = center });
        }
        
        // Minor period: Moonset (moon disappears below the horizon)
        // Similar to moonrise, 1-hour period centered at +30 minutes
        if (moonset.HasValue)
        {
            var center = moonset.Value.AddMinutes(30);
            minorUtc.Add(new SolunarPeriod { Type = SolunarPeriodType.Moonset, Start = moonset.Value, End = moonset.Value.AddHours(1), Center = center });
        }

        // STEP 3: Calculate hourly activity scores (0-100) for each hour of the day
        var hourly = new List<HourlyActivity>();
        
        // Loop through all 24 hours (0-23)
        for (int hour = 0; hour <= 23; hour++)
        {
            // Convert local hour to UTC for calculations
            // We evaluate at the middle of each hour (e.g., 12:30 for hour 12)
            var t = TimeZoneHelper.LocalMidHourToUtc(input.Date, hour, tz);
            double score = 0;

            // Calculate contribution from major periods using Gaussian decay
            // Maximum contribution is 100 points within ±90 minutes of period center
            foreach (var p in majorUtc)
            {
                var dist = Math.Abs((t - p.Center).TotalMinutes);  // Distance in minutes from period center
                if (dist <= 90)  // Only contribute if within 90-minute window
                {
                    // Gaussian decay: score decreases as distance from center increases
                    // Formula: 100 * e^(-(dist²/800))
                    // At center (dist=0): 100 points
                    // At ±45 min: ~60 points
                    // At ±90 min: ~10 points
                    var contrib = 100 * Math.Exp(-(dist * dist) / 800.0);
                    score = Math.Max(score, contrib);  // Take the maximum if multiple periods overlap
                }
            }
            
            // Calculate contribution from minor periods using steeper Gaussian decay
            // Maximum contribution is 70 points within ±45 minutes of period center
            foreach (var p in minorUtc)
            {
                var dist = Math.Abs((t - p.Center).TotalMinutes);
                if (dist <= 45)  // Only contribute if within 45-minute window
                {
                    // Steeper decay for minor periods: 70 * e^(-(dist²/200))
                    // At center: 70 points
                    // At ±30 min: ~15 points
                    // At ±45 min: ~3 points
                    var contrib = 70 * Math.Exp(-(dist * dist) / 200.0);
                    score = Math.Max(score, contrib);
                }
            }

            // STEP 4: Apply moon phase multiplier (0.9 - 1.1)
            // New moon and full moon are considered more active periods
            var phaseMult = GetPhaseMultiplier(result.MoonPhase.Phase);
            score *= phaseMult;

            // STEP 5: Apply time of day multiplier (0.8 - 1.1)
            // Activity is higher during dawn/dusk and lower during midday
            var dayMult = GetDayTimeMultiplier(t, sunrise, sunset);
            score *= dayMult;

            // STEP 6: Add overlap bonus if this hour falls within both major and minor periods
            // When periods overlap, activity is considered even higher (+18 bonus points)
            if (IsInAny(t, majorUtc) && IsInAny(t, minorUtc))
            {
                score += 18;
            }

            // Clamp final score to 0-100 range and round to integer
            int final = (int)Math.Round(Math.Clamp(score, 0, 100));
            hourly.Add(new HourlyActivity { Hour = hour, Score = final });
        }
        
        // Store hourly activity and calculate overall daily rating
        result.HourlyActivity = hourly;
        result.SolunarRating = GetOverallRating(hourly);

        // STEP 7: Convert all periods from UTC to local time zone for output
        // Users expect to see times in their local time zone
        foreach (var p in majorUtc)
        {
            result.MajorTimes.Add(new SolunarPeriod
            {
                Type = p.Type,
                Start = TimeZoneHelper.ToLocal(p.Start, tz),
                End = TimeZoneHelper.ToLocal(p.End, tz),
                Center = TimeZoneHelper.ToLocal(p.Center, tz)
            });
        }
        foreach (var p in minorUtc)
        {
            result.MinorTimes.Add(new SolunarPeriod
            {
                Type = p.Type,
                Start = TimeZoneHelper.ToLocal(p.Start, tz),
                End = TimeZoneHelper.ToLocal(p.End, tz),
                Center = TimeZoneHelper.ToLocal(p.Center, tz)
            });
        }

        return result;
    }

    /// <summary>
    /// Checks if a given time falls within any of the specified solunar periods.
    /// </summary>
    /// <param name="t">The time to check (in UTC).</param>
    /// <param name="periods">The collection of solunar periods to check against.</param>
    /// <returns>True if the time falls within at least one period; otherwise, false.</returns>
    /// <remarks>
    /// Used to determine overlap bonuses when calculating hourly activity scores.
    /// A time is considered "in" a period if it falls between the period's Start and End (inclusive).
    /// </remarks>
    private static bool IsInAny(DateTime t, IEnumerable<SolunarPeriod> periods)
    {
        // Iterate through all periods to find if time t is within any of them
        foreach (var p in periods)
        {
            // Check if t is within the period's time range (inclusive)
            if (t >= p.Start && t <= p.End) return true;
        }
        // Time doesn't fall within any period
        return false;
    }

    /// <summary>
    /// Returns a multiplier based on the current moon phase to adjust activity scores.
    /// </summary>
    /// <param name="phaseName">The name of the moon phase (e.g., "New Moon", "Full Moon", "First Quarter").</param>
    /// <returns>
    /// A multiplier value between 0.9 and 1.1:
    /// - New Moon: 1.05 (slightly enhanced activity)
    /// - Full Moon: 1.10 (highest activity boost)
    /// - First Quarter: 1.00 (neutral)
    /// - Last Quarter: 0.95 (slightly reduced activity)
    /// - Other phases: 1.00 (neutral)
    /// </returns>
    /// <remarks>
    /// According to solunar theory, full moons and new moons trigger increased activity in fish and game.
    /// The multiplier ranges from 0.9 to 1.1, providing a ±10% adjustment to activity scores.
    /// </remarks>
    private static double GetPhaseMultiplier(string phaseName)
    {
        // Handle null or empty phase names
        if (string.IsNullOrWhiteSpace(phaseName)) return 1.0;
        
        // Normalize the phase name: remove spaces and convert to lowercase for consistent matching
        phaseName = phaseName.Replace(" ", string.Empty).ToLowerInvariant();
        
        // Return appropriate multiplier based on moon phase
        return phaseName switch
        {
            "newmoon" => 1.05,      // New moon: slight boost (5%)
            "fullmoon" => 1.10,     // Full moon: highest boost (10%)
            "firstquarter" => 1.00, // First quarter: neutral
            "lastquarter" => 0.95,  // Last quarter: slight reduction (-5%)
            _ => 1.0                // All other phases (waxing/waning crescent/gibbous): neutral
        };
    }

    /// <summary>
    /// Returns a multiplier based on the time of day relative to sunrise and sunset.
    /// </summary>
    /// <param name="tUtc">The time to evaluate (in UTC).</param>
    /// <param name="sunriseUtc">The sunrise time (in UTC), or null if no sunrise occurs.</param>
    /// <param name="sunsetUtc">The sunset time (in UTC), or null if no sunset occurs.</param>
    /// <returns>
    /// A multiplier value between 0.9 and 1.1:
    /// - Within 30 minutes of sunrise or sunset: 1.10 (dawn/dusk peak)
    /// - Within 2 hours after sunrise or sunset: 1.05 (extended dawn/dusk)
    /// - Within 1 hour of solar noon (midday): 0.90 (lowest daytime activity)
    /// - Daytime (between sunrise and sunset): 1.00 (neutral)
    /// - Nighttime (outside sunrise-sunset): 0.95 (slightly reduced)
    /// </returns>
    /// <remarks>
    /// Fish and game are typically most active during dawn and dusk (crepuscular activity).
    /// Activity is lowest during midday when the sun is at its highest.
    /// This multiplier accounts for these natural activity patterns.
    /// </remarks>
    private static double GetDayTimeMultiplier(DateTime tUtc, DateTime? sunriseUtc, DateTime? sunsetUtc)
    {
        // If sunrise or sunset data is unavailable (e.g., polar regions), return neutral multiplier
        if (sunriseUtc == null || sunsetUtc == null)
        {
            return 1.0;
        }

        // Helper function to calculate time distance in minutes
        double MinutesTo(DateTime? evt) => Math.Abs((tUtc - evt!.Value).TotalMinutes);

        // Calculate distance from sunrise and sunset
        var sunriseDelta = MinutesTo(sunriseUtc);
        var sunsetDelta = MinutesTo(sunsetUtc);

        // PEAK ACTIVITY: Within 30 minutes of sunrise or sunset (dawn/dusk)
        // This is the "golden hour" for fishing and hunting
        if (sunriseDelta <= 30 || sunsetDelta <= 30) return 1.10;
        
        // ENHANCED ACTIVITY: Within 2 hours after sunrise or sunset
        // Extended crepuscular period with elevated activity
        if ((tUtc >= sunriseUtc && tUtc <= sunriseUtc.Value.AddHours(2)) ||
            (tUtc >= sunsetUtc && tUtc <= sunsetUtc.Value.AddHours(2))) return 1.05;

        // REDUCED ACTIVITY: Around solar noon (midday)
        // Calculate approximate midday as the midpoint between sunrise and sunset
        var midday = sunriseUtc.Value + TimeSpan.FromTicks((sunsetUtc.Value - sunriseUtc.Value).Ticks / 2);
        
        // Within 1 hour of midday: lowest activity multiplier
        if (Math.Abs((tUtc - midday).TotalMinutes) <= 60) return 0.90;

        // NEUTRAL/SLIGHTLY REDUCED: General day vs night adjustment
        // Determine if current time is during daytime or nighttime
        bool isDay = tUtc >= sunriseUtc && tUtc <= sunsetUtc;
        
        // Daytime (outside special periods): neutral
        // Nighttime: slightly reduced (but remember lunar periods can boost this)
        return isDay ? 1.0 : 0.95;
    }

    /// <summary>
    /// Determines the overall solunar rating for the day based on average hourly activity scores.
    /// </summary>
    /// <param name="hourly">The list of hourly activity scores for all 24 hours.</param>
    /// <returns>
    /// A string rating:
    /// - "Excellent": Average score ≥ 80 (highly active day)
    /// - "Good": Average score ≥ 60 (moderately active day)
    /// - "Fair": Average score ≥ 40 (average activity)
    /// - "Poor": Average score &lt; 40 (low activity day)
    /// </returns>
    /// <remarks>
    /// The rating provides a quick summary of the overall fishing/hunting potential for the day.
    /// It's based on the mean of all 24 hourly scores.
    /// </remarks>
    private static string GetOverallRating(List<HourlyActivity> hourly)
    {
        // Calculate the average score across all 24 hours
        double avg = hourly.Average(h => h.Score);
        
        // Classify the day based on average activity score
        return avg switch
        {
            >= 80 => "Excellent",  // Very high activity day
            >= 60 => "Good",       // Above average activity
            >= 40 => "Fair",       // Average activity
            _ => "Poor"            // Below average activity
        };
    }
}
