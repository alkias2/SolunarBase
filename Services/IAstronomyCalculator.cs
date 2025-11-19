using SolunarBase.Models;

namespace SolunarBase.Services;

public interface IAstronomyCalculator
{
    (DateTime? SunriseUtc, DateTime? SunsetUtc) GetSunTimes(double latitude, double longitude, DateOnly date);
    (DateTime? MoonriseUtc, DateTime? MoonsetUtc) GetMoonTimes(double latitude, double longitude, DateOnly date);
    (DateTime? UpperTransitUtc, DateTime? LowerTransitUtc) GetLunarTransits(double latitude, double longitude, DateOnly date);
    MoonPhaseInfo GetMoonPhase(double latitude, double longitude, DateOnly date);
}
