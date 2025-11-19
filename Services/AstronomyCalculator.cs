using CoordinateSharp;
using SolunarBase.Models;

namespace SolunarBase.Services;

public class AstronomyCalculator : IAstronomyCalculator
{
    public (DateTime? SunriseUtc, DateTime? SunsetUtc) GetSunTimes(double latitude, double longitude, DateOnly date)
    {
        var c = new Celestial(latitude, longitude, date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        return (c.SunRise, c.SunSet);
    }

    public (DateTime? MoonriseUtc, DateTime? MoonsetUtc) GetMoonTimes(double latitude, double longitude, DateOnly date)
    {
        var c = new Celestial(latitude, longitude, date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        return (c.MoonRise, c.MoonSet);
    }

    public (DateTime? UpperTransitUtc, DateTime? LowerTransitUtc) GetLunarTransits(double latitude, double longitude, DateOnly date)
    {
        // Sample throughout the day to find altitude extrema (approximate upper/lower transit)
        DateTime start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        DateTime end = start.AddDays(1);
        TimeSpan coarseStep = TimeSpan.FromMinutes(5);

        DateTime? maxT = null, minT = null;
        double maxAlt = double.NegativeInfinity;
        double minAlt = double.PositiveInfinity;

        for (var t = start; t < end; t += coarseStep)
        {
            var cel = new Celestial(latitude, longitude, t);
            double alt = cel.MoonAltitude;
            if (alt > maxAlt)
            {
                maxAlt = alt;
                maxT = t;
            }
            if (alt < minAlt)
            {
                minAlt = alt;
                minT = t;
            }
        }

        // Refine around the extrema with 1-minute steps ±30 minutes
        DateTime? refineMax = RefineExtremum(latitude, longitude, maxT, true);
        DateTime? refineMin = RefineExtremum(latitude, longitude, minT, false);

        return (refineMax, refineMin);
    }

    public MoonPhaseInfo GetMoonPhase(double latitude, double longitude, DateOnly date)
    {
        var cel = new Celestial(latitude, longitude, date.ToDateTime(new TimeOnly(12,0), DateTimeKind.Utc));
        var illum = cel.MoonIllum;
        return new MoonPhaseInfo
        {
            Phase = illum.PhaseName,
            Illumination = Math.Clamp(illum.Fraction, 0, 1)
        };
    }

    private static DateTime? RefineExtremum(double latitude, double longitude, DateTime? seed, bool seekMax)
    {
        if (seed == null) return null;
        var windowStart = seed.Value.AddMinutes(-30);
        var windowEnd = seed.Value.AddMinutes(30);
        var step = TimeSpan.FromMinutes(1);
        DateTime? bestT = null;
        double bestAlt = seekMax ? double.NegativeInfinity : double.PositiveInfinity;

        for (var t = windowStart; t <= windowEnd; t += step)
        {
            var cel = new Celestial(latitude, longitude, t);
            double alt = cel.MoonAltitude;
            if (seekMax)
            {
                if (alt > bestAlt)
                {
                    bestAlt = alt;
                    bestT = t;
                }
            }
            else
            {
                if (alt < bestAlt)
                {
                    bestAlt = alt;
                    bestT = t;
                }
            }
        }
        return bestT;
    }
}
