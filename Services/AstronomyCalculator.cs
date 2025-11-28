using CosineKitty;
using SolunarBase.Models;

namespace SolunarBase.Services;

/// <summary>
/// Provides astronomical calculations using the CosineKitty.AstronomyEngine library.
/// Calculates sun and moon positions, rise/set times, phases, and other celestial events.
/// </summary>
public class AstronomyCalculator : IAstronomyCalculator
{
    #region Public

    /// <summary>
    /// Calculates comprehensive astronomical data for the Sun and Moon.
    /// </summary>
    public AstronomicalData GetAstronomicalData(double latitude, double longitude, DateOnly date, string timeZoneId) {
        var astroData = new AstronomicalData();
        
        // Get timezone info for conversion
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        
        // Calculate Sun data
        var sunTimes = GetSunTimes(latitude, longitude, date);
        astroData.Sun.RiseUtc = sunTimes.SunriseUtc;
        astroData.Sun.RiseLocal = sunTimes.SunriseUtc.HasValue 
            ? TimeZoneInfo.ConvertTimeFromUtc(sunTimes.SunriseUtc.Value, timeZone) 
            : null;
        astroData.Sun.SetUtc = sunTimes.SunsetUtc;
        astroData.Sun.SetLocal = sunTimes.SunsetUtc.HasValue 
            ? TimeZoneInfo.ConvertTimeFromUtc(sunTimes.SunsetUtc.Value, timeZone) 
            : null;
        
        // Calculate Sun culmination (highest point)
        var sunCulm = GetSunCulmination(latitude, longitude, date);
        astroData.Sun.CulminationUtc = sunCulm.TimeUtc;
        astroData.Sun.CulminationLocal = sunCulm.TimeUtc.HasValue 
            ? TimeZoneInfo.ConvertTimeFromUtc(sunCulm.TimeUtc.Value, timeZone) 
            : null;
        astroData.Sun.CulminationAltitudeDegrees = sunCulm.Altitude;
        astroData.Sun.CulminationAzimuthDegrees = sunCulm.Azimuth;
        
        // Calculate Moon data
        var moonTimes = GetMoonTimes(latitude, longitude, date);
        astroData.Moon.RiseUtc = moonTimes.MoonriseUtc;
        astroData.Moon.RiseLocal = moonTimes.MoonriseUtc.HasValue 
            ? TimeZoneInfo.ConvertTimeFromUtc(moonTimes.MoonriseUtc.Value, timeZone) 
            : null;
        astroData.Moon.SetUtc = moonTimes.MoonsetUtc;
        astroData.Moon.SetLocal = moonTimes.MoonsetUtc.HasValue 
            ? TimeZoneInfo.ConvertTimeFromUtc(moonTimes.MoonsetUtc.Value, timeZone) 
            : null;
        
        // Calculate Moon transits
        var moonTransits = GetLunarTransits(latitude, longitude, date);
        astroData.Moon.UpperTransitUtc = moonTransits.UpperTransitUtc;
        astroData.Moon.UpperTransitLocal = moonTransits.UpperTransitUtc.HasValue 
            ? TimeZoneInfo.ConvertTimeFromUtc(moonTransits.UpperTransitUtc.Value, timeZone) 
            : null;
        astroData.Moon.LowerTransitUtc = moonTransits.LowerTransitUtc;
        astroData.Moon.LowerTransitLocal = moonTransits.LowerTransitUtc.HasValue 
            ? TimeZoneInfo.ConvertTimeFromUtc(moonTransits.LowerTransitUtc.Value, timeZone) 
            : null;
        
        // Get transit altitudes and azimuths
        if (moonTransits.UpperTransitUtc.HasValue) {
            var upperHor = GetHorizontalCoordinates(Body.Moon, latitude, longitude, moonTransits.UpperTransitUtc.Value);
            astroData.Moon.UpperTransitAltitudeDegrees = upperHor.Altitude;
            astroData.Moon.UpperTransitAzimuthDegrees = upperHor.Azimuth;
        }
        
        if (moonTransits.LowerTransitUtc.HasValue) {
            var lowerHor = GetHorizontalCoordinates(Body.Moon, latitude, longitude, moonTransits.LowerTransitUtc.Value);
            astroData.Moon.LowerTransitAltitudeDegrees = lowerHor.Altitude;
        }
        
        // Calculate Moon phase
        var moonPhase = GetMoonPhase(latitude, longitude, date);
        astroData.Moon.Phase = moonPhase.Phase;
        astroData.Moon.Illumination = moonPhase.Illumination;
        
        // Get detailed moon phase angle and distance
        var phaseDetails = GetMoonPhaseDetails(date);
        astroData.Moon.PhaseAngle = phaseDetails.PhaseAngle;
        astroData.Moon.DistanceKm = phaseDetails.DistanceKm;
        
        return astroData;
    }

    /// <summary>
    /// Calculates the sunrise and sunset times using AstronomyEngine.
    /// </summary>
    public (DateTime? SunriseUtc, DateTime? SunsetUtc) GetSunTimes(double latitude, double longitude, DateOnly date) {
        var observer = new Observer(latitude, longitude, 0.0);
        var startTime = new AstroTime(date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        
        // Search for sunrise (Direction.Rise = 1)
        AstroTime? sunrise = null;
        AstroTime? sunset = null;
        
        try {
            sunrise = Astronomy.SearchRiseSet(Body.Sun, observer, Direction.Rise, startTime, 1.0, 0.0);
        }
        catch {
            // No sunrise found (polar regions, etc.)
        }
        
        try {
            sunset = Astronomy.SearchRiseSet(Body.Sun, observer, Direction.Set, startTime, 1.0, 0.0);
        }
        catch {
            // No sunset found
        }
        
        return (sunrise?.ToUtcDateTime(), sunset?.ToUtcDateTime());
    }

    /// <summary>
    /// Calculates the moonrise and moonset times using AstronomyEngine.
    /// </summary>
    public (DateTime? MoonriseUtc, DateTime? MoonsetUtc) GetMoonTimes(double latitude, double longitude, DateOnly date) {
        var observer = new Observer(latitude, longitude, 0.0);
        var startTime = new AstroTime(date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        
        AstroTime? moonrise = null;
        AstroTime? moonset = null;
        
        try {
            moonrise = Astronomy.SearchRiseSet(Body.Moon, observer, Direction.Rise, startTime, 1.0, 0.0);
        }
        catch {
            // No moonrise found
        }
        
        try {
            moonset = Astronomy.SearchRiseSet(Body.Moon, observer, Direction.Set, startTime, 1.0, 0.0);
        }
        catch {
            // No moonset found
        }
        
        return (moonrise?.ToUtcDateTime(), moonset?.ToUtcDateTime());
    }

    /// <summary>
    /// Calculates the lunar transit times (upper and lower transit) using AstronomyEngine.
    /// Upper transit = culmination (highest point), Lower transit = anti-culmination (lowest point).
    /// Returns the transits that are closest to noon of the specified day (in UTC).
    /// This ensures we get the correct pair of transits for a 24-hour period.
    /// </summary>
    public (DateTime? UpperTransitUtc, DateTime? LowerTransitUtc) GetLunarTransits(double latitude, double longitude, DateOnly date) {
        var observer = new Observer(latitude, longitude, 0.0);
        
        // Target time is noon of the specified day
        var targetTime = date.ToDateTime(new TimeOnly(12, 0), DateTimeKind.Utc);
        
        // Search starting from the previous day to catch all possible transits
        var searchStart = new AstroTime(date.AddDays(-1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        
        // Find all transits within a 3-day window and pick the ones closest to target
        List<(DateTime time, bool isUpper)> allTransits = new();
        
        // Search for up to 4 transits (2 upper + 2 lower in the window)
        var currentSearch = searchStart;
        for (int i = 0; i < 4; i++) {
            try {
                var upper = Astronomy.SearchHourAngle(Body.Moon, observer, 0.0, currentSearch, 3);
                allTransits.Add((upper.time.ToUtcDateTime(), true));
                currentSearch = upper.time.AddDays(0.1); // Move search forward
            }
            catch { break; }
        }
        
        currentSearch = searchStart;
        for (int i = 0; i < 4; i++) {
            try {
                var lower = Astronomy.SearchHourAngle(Body.Moon, observer, 12.0, currentSearch, 3);
                allTransits.Add((lower.time.ToUtcDateTime(), false));
                currentSearch = lower.time.AddDays(0.1); // Move search forward
            }
            catch { break; }
        }
        
        // Find the upper and lower transits closest to noon of the target day
        var upperClosest = allTransits
            .Where(t => t.isUpper)
            .OrderBy(t => Math.Abs((t.time - targetTime).TotalHours))
            .FirstOrDefault();
            
        var lowerClosest = allTransits
            .Where(t => !t.isUpper)
            .OrderBy(t => Math.Abs((t.time - targetTime).TotalHours))
            .FirstOrDefault();
        
        return (
            upperClosest.time != default ? upperClosest.time : null,
            lowerClosest.time != default ? lowerClosest.time : null
        );
    }
    
    /// <summary>
    /// Calculates the moon phase and illumination using AstronomyEngine.
    /// </summary>
    public MoonPhaseInfo GetMoonPhase(double latitude, double longitude, DateOnly date) {
        // Use noon UTC for phase calculation to get a representative value for the day
        var noonTime = new AstroTime(date.ToDateTime(new TimeOnly(12, 0), DateTimeKind.Utc));
        
        // Get the moon phase angle (0-360 degrees)
        double phaseAngle = Astronomy.MoonPhase(noonTime);
        
        // Get illumination info
        var illum = Astronomy.Illumination(Body.Moon, noonTime);
        
        // Determine phase name based on phase angle
        string phaseName = GetPhaseName(phaseAngle);
        
        return new MoonPhaseInfo {
            Phase = phaseName,
            Illumination = Math.Clamp(illum.phase_fraction, 0.0, 1.0)
        };
    }

    #endregion

    #region Privates

    /// <summary>
    /// Gets the Sun's culmination (highest point) time and position.
    /// </summary>
    private (DateTime? TimeUtc, double? Altitude, double? Azimuth) GetSunCulmination(double latitude, double longitude, DateOnly date) {
        var observer = new Observer(latitude, longitude, 0.0);
        var startTime = new AstroTime(date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        
        try {
            // Search for hour angle = 0 (culmination)
            var culmination = Astronomy.SearchHourAngle(Body.Sun, observer, 0.0, startTime, 1);
            
            return (
                culmination.time.ToUtcDateTime(),
                culmination.hor.altitude,
                culmination.hor.azimuth
            );
        }
        catch {
            return (null, null, null);
        }
    }

    /// <summary>
    /// Gets detailed moon phase information including distance.
    /// </summary>
    private (double PhaseAngle, double DistanceKm) GetMoonPhaseDetails(DateOnly date) {
        var noonTime = new AstroTime(date.ToDateTime(new TimeOnly(12, 0), DateTimeKind.Utc));
        
        // Get the moon phase angle
        double phaseAngle = Astronomy.MoonPhase(noonTime);
        
        // Get geocentric moon vector to calculate distance
        var moonVec = Astronomy.GeoMoon(noonTime);
        double distanceAu = moonVec.Length();
        double distanceKm = distanceAu * Astronomy.KM_PER_AU;
        
        return (phaseAngle, distanceKm);
    }

    /// <summary>
    /// Gets horizontal coordinates (altitude, azimuth) for a body at a specific time.
    /// </summary>
    private (double Altitude, double Azimuth) GetHorizontalCoordinates(Body body, double latitude, double longitude, DateTime utcTime) {
        var observer = new Observer(latitude, longitude, 0.0);
        var time = new AstroTime(utcTime);
        
        // Get equatorial coordinates
        var equ = Astronomy.Equator(body, time, observer, EquatorEpoch.OfDate, Aberration.Corrected);
        
        // Convert to horizontal coordinates
        var hor = Astronomy.Horizon(time, observer, equ.ra, equ.dec, Refraction.Normal);
        
        return (hor.altitude, hor.azimuth);
    }

    /// <summary>
    /// Determines the phase name based on the phase angle.
    /// Phase angle: 0 = New Moon, 90 = First Quarter, 180 = Full Moon, 270 = Third Quarter.
    /// </summary>
    private static string GetPhaseName(double phaseAngle) {
        // Normalize to [0, 360)
        phaseAngle = phaseAngle % 360.0;
        if (phaseAngle < 0) phaseAngle += 360.0;
        
        return phaseAngle switch {
            >= 0 and < 22.5 => "New Moon",
            >= 22.5 and < 67.5 => "Waxing Crescent",
            >= 67.5 and < 112.5 => "First Quarter",
            >= 112.5 and < 157.5 => "Waxing Gibbous",
            >= 157.5 and < 202.5 => "Full Moon",
            >= 202.5 and < 247.5 => "Waning Gibbous",
            >= 247.5 and < 292.5 => "Third Quarter",
            >= 292.5 and < 337.5 => "Waning Crescent",
            _ => "New Moon"
        };
    }

    #endregion
}
