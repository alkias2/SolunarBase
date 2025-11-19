using SolunarBase.Models;
using SolunarBase.Utils;

namespace SolunarBase.Services;

public class SolunarCalculator : ISolunarCalculator
{
    private readonly IAstronomyCalculator _astro;

    public SolunarCalculator(IAstronomyCalculator astro)
    {
        _astro = astro;
    }

    public SolunarResult Calculate(SolunarInput input)
    {
        var tz = TimeZoneHelper.Resolve(input.TimeZoneId);
        var result = new SolunarResult
        {
            Date = input.Date,
            Location = new Location { Latitude = input.Latitude, Longitude = input.Longitude },
            TimeZoneId = tz.Id
        };

        var (sunrise, sunset) = _astro.GetSunTimes(input.Latitude, input.Longitude, input.Date);
        var (moonrise, moonset) = _astro.GetMoonTimes(input.Latitude, input.Longitude, input.Date);
        var (upper, lower) = _astro.GetLunarTransits(input.Latitude, input.Longitude, input.Date);
        result.MoonPhase = _astro.GetMoonPhase(input.Latitude, input.Longitude, input.Date);

        // Build UTC periods first
        var majorUtc = new List<SolunarPeriod>();
        var minorUtc = new List<SolunarPeriod>();
        if (upper.HasValue)
        {
            var start = upper.Value.AddHours(-1);
            var end = upper.Value.AddHours(1);
            majorUtc.Add(new SolunarPeriod { Type = SolunarPeriodType.UpperTransit, Start = start, End = end, Center = upper.Value });
        }
        if (lower.HasValue)
        {
            var start = lower.Value.AddHours(-1);
            var end = lower.Value.AddHours(1);
            majorUtc.Add(new SolunarPeriod { Type = SolunarPeriodType.LowerTransit, Start = start, End = end, Center = lower.Value });
        }
        if (moonrise.HasValue)
        {
            var center = moonrise.Value.AddMinutes(30);
            minorUtc.Add(new SolunarPeriod { Type = SolunarPeriodType.Moonrise, Start = moonrise.Value, End = moonrise.Value.AddHours(1), Center = center });
        }
        if (moonset.HasValue)
        {
            var center = moonset.Value.AddMinutes(30);
            minorUtc.Add(new SolunarPeriod { Type = SolunarPeriodType.Moonset, Start = moonset.Value, End = moonset.Value.AddHours(1), Center = center });
        }

        // Hourly scores
        var hourly = new List<HourlyActivity>();
        for (int hour = 0; hour <= 23; hour++)
        {
            // Use local hour (Europe/Athens by default), convert to UTC for calculations
            var t = TimeZoneHelper.LocalMidHourToUtc(input.Date, hour, tz);
            double score = 0;

            foreach (var p in majorUtc)
            {
                var dist = Math.Abs((t - p.Center).TotalMinutes);
                if (dist <= 90)
                {
                    var contrib = 100 * Math.Exp(-(dist * dist) / 800.0);
                    score = Math.Max(score, contrib);
                }
            }
            foreach (var p in minorUtc)
            {
                var dist = Math.Abs((t - p.Center).TotalMinutes);
                if (dist <= 45)
                {
                    var contrib = 70 * Math.Exp(-(dist * dist) / 200.0);
                    score = Math.Max(score, contrib);
                }
            }

            // Phase multiplier (0.9 - 1.1)
            var phaseMult = GetPhaseMultiplier(result.MoonPhase.Phase);
            score *= phaseMult;

            // Daytime multiplier (0.8 - 1.1)
            var dayMult = GetDayTimeMultiplier(t, sunrise, sunset);
            score *= dayMult;

            // Overlap bonus
            if (IsInAny(t, majorUtc) && IsInAny(t, minorUtc))
            {
                score += 18;
            }

            int final = (int)Math.Round(Math.Clamp(score, 0, 100));
            hourly.Add(new HourlyActivity { Hour = hour, Score = final });
        }
        result.HourlyActivity = hourly;
        result.SolunarRating = GetOverallRating(hourly);

        // Convert periods to local time for output
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

    private static bool IsInAny(DateTime t, IEnumerable<SolunarPeriod> periods)
    {
        foreach (var p in periods)
        {
            if (t >= p.Start && t <= p.End) return true;
        }
        return false;
    }

    private static double GetPhaseMultiplier(string phaseName)
    {
        if (string.IsNullOrWhiteSpace(phaseName)) return 1.0;
        phaseName = phaseName.Replace(" ", string.Empty).ToLowerInvariant();
        return phaseName switch
        {
            "newmoon" => 1.05,
            "fullmoon" => 1.10,
            "firstquarter" => 1.00,
            "lastquarter" => 0.95,
            _ => 1.0
        };
    }

    private static double GetDayTimeMultiplier(DateTime tUtc, DateTime? sunriseUtc, DateTime? sunsetUtc)
    {
        if (sunriseUtc == null || sunsetUtc == null)
        {
            return 1.0;
        }

        double MinutesTo(DateTime? evt) => Math.Abs((tUtc - evt!.Value).TotalMinutes);

        var sunriseDelta = MinutesTo(sunriseUtc);
        var sunsetDelta = MinutesTo(sunsetUtc);

        if (sunriseDelta <= 30 || sunsetDelta <= 30) return 1.10;
        if ((tUtc >= sunriseUtc && tUtc <= sunriseUtc.Value.AddHours(2)) ||
            (tUtc >= sunsetUtc && tUtc <= sunsetUtc.Value.AddHours(2))) return 1.05;

        // Approx midday as halfway between sunrise/sunset
        var midday = sunriseUtc.Value + TimeSpan.FromTicks((sunsetUtc.Value - sunriseUtc.Value).Ticks / 2);
        if (Math.Abs((tUtc - midday).TotalMinutes) <= 60) return 0.90;

        // Night vs day slight adjustment
        bool isDay = tUtc >= sunriseUtc && tUtc <= sunsetUtc;
        return isDay ? 1.0 : 0.95;
    }

    private static string GetOverallRating(List<HourlyActivity> hourly)
    {
        double avg = hourly.Average(h => h.Score);
        return avg switch
        {
            >= 80 => "Excellent",
            >= 60 => "Good",
            >= 40 => "Fair",
            _ => "Poor"
        };
    }
}
