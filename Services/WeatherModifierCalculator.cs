using SolunarBase.Models;

namespace SolunarBase.Services;

/// <summary>
/// Calculates weather-based activity modifiers for fish behavior.
/// </summary>
/// <remarks>
/// Weather conditions significantly impact fish feeding patterns.
/// This calculator converts raw weather data into activity score modifiers.
/// All modifier calculations are based on empirical fishing data and biological research.
/// </remarks>
public class WeatherModifierCalculator
{
    private readonly ModifierWeights _weights;

    /// <summary>
    /// Initializes a new instance of the WeatherModifierCalculator.
    /// </summary>
    /// <param name="weights">The weighting configuration for different weather factors.</param>
    public WeatherModifierCalculator(ModifierWeights weights)
    {
        _weights = weights;
    }

    /// <summary>
    /// Calculates the total weather modifier for a specific hour.
    /// </summary>
    /// <param name="weather">The weather data for the hour being evaluated.</param>
    /// <param name="previousWeather">The weather data from the previous hour (for trend analysis), or null if not available.</param>
    /// <returns>A modifier score ranging approximately from -50 to +50 points.</returns>
    /// <remarks>
    /// The modifier is a weighted sum of individual weather factor scores.
    /// Each factor contributes based on its configured weight and biological impact.
    /// </remarks>
    public double CalculateModifier(WeatherData weather, WeatherData? previousWeather)
    {
        double modifier = 0;

        // WATER TEMPERATURE: Most critical factor for fish activity
        // Different species have optimal ranges, but generally 18-24°C is good for many species
        modifier += CalculateWaterTemperatureScore(weather.WaterTemperature) * _weights.Weather.WaterTemperature;

        // PRESSURE: Rising pressure often means better fishing, falling can be good too
        // Rapid changes affect fish behavior significantly
        modifier += CalculatePressureScore(weather.Pressure, previousWeather?.Pressure) * _weights.Weather.Pressure;

        // WIND: Moderate wind creates surface disturbance and oxygenation
        // Too much wind makes fishing difficult and can reduce activity
        modifier += CalculateWindScore(weather.WindSpeed, weather.WindDirection) * _weights.Weather.Wind;

        // CLOUD COVER: Overcast conditions often increase activity
        // Fish feel more secure and feed more actively
        modifier += CalculateCloudCoverScore(weather.CloudCover) * _weights.Weather.CloudCover;

        // WAVES/SWELL/CURRENT: Moderate action increases feeding
        // Too much makes it difficult, too little can be stagnant
        modifier += CalculateWaveScore(weather.WaveHeight, weather.CurrentSpeed) * _weights.Weather.Waves;

        // AIR TEMPERATURE: Less critical than water temp but still affects surface activity
        modifier += CalculateAirTemperatureScore(weather.AirTemperature) * _weights.Weather.AirTemperature;

        // HUMIDITY: Minimal impact but can indicate weather systems
        modifier += CalculateHumidityScore(weather.Humidity) * _weights.Weather.Humidity;

        return modifier;
    }

    /// <summary>
    /// Calculates the water temperature modifier.
    /// </summary>
    /// <param name="waterTemp">Water temperature in Celsius.</param>
    /// <returns>Score from -15 to +15.</returns>
    /// <remarks>
    /// Optimal range: 18-24°C (+15 bonus)
    /// Good range: 15-27°C (+5 to +10)
    /// Poor range: below 12°C or above 30°C (-10 to -15)
    /// </remarks>
    private static double CalculateWaterTemperatureScore(double waterTemp)
    {
        // Optimal range for many fish species: 18-24°C
        if (waterTemp >= 18 && waterTemp <= 24)
            return 15;

        // Good range: 15-18°C or 24-27°C
        if (waterTemp >= 15 && waterTemp < 18)
            return 5 + (waterTemp - 15) * 3.33; // Linear increase from +5 to +15

        if (waterTemp > 24 && waterTemp <= 27)
            return 15 - (waterTemp - 24) * 3.33; // Linear decrease from +15 to +5

        // Acceptable range: 12-15°C or 27-30°C
        if (waterTemp >= 12 && waterTemp < 15)
            return -5 + (waterTemp - 12) * 3.33; // From -5 to +5

        if (waterTemp > 27 && waterTemp <= 30)
            return 5 - (waterTemp - 27) * 3.33; // From +5 to -5

        // Poor conditions: below 12°C or above 30°C
        if (waterTemp < 12)
            return Math.Max(-15, -5 - (12 - waterTemp) * 2); // Down to -15

        // Above 30°C
        return Math.Max(-15, -5 - (waterTemp - 30) * 2); // Down to -15
    }

    /// <summary>
    /// Calculates the atmospheric pressure modifier.
    /// </summary>
    /// <param name="pressure">Current pressure in millibars.</param>
    /// <param name="previousPressure">Previous hour's pressure, or null if not available.</param>
    /// <returns>Score from -15 to +15.</returns>
    /// <remarks>
    /// Rising pressure: +10 to +15 (excellent)
    /// Stable high pressure (1013-1023 mb): +5 to +10 (good)
    /// Falling slowly: 0 to +5 (can still be good)
    /// Falling rapidly: -10 to -15 (poor)
    /// Very low pressure (&lt;1000 mb): -10 to -15 (poor)
    /// </remarks>
    private static double CalculatePressureScore(double pressure, double? previousPressure)
    {
        double score = 0;

        // Calculate pressure trend if previous data available
        if (previousPressure.HasValue)
        {
            double change = pressure - previousPressure.Value;

            // Rising pressure: positive (fish become more active)
            if (change > 2) score += 15; // Rapid rise
            else if (change > 0.5) score += 10; // Moderate rise
            else if (change > 0) score += 5; // Slight rise

            // Falling pressure: can be positive initially, then negative
            else if (change > -0.5) score += 3; // Stable or slight fall
            else if (change > -2) score -= 5; // Moderate fall
            else score -= 15; // Rapid fall (storm approaching)
        }

        // Absolute pressure value adjustment
        if (pressure >= 1013 && pressure <= 1023)
            score += 5; // Ideal range (high pressure system)
        else if (pressure < 1000)
            score -= 10; // Very low pressure (storm)
        else if (pressure > 1030)
            score -= 5; // Very high pressure (can reduce activity)

        return Math.Clamp(score, -15, 15);
    }

    /// <summary>
    /// Calculates the wind modifier.
    /// </summary>
    /// <param name="windSpeed">Wind speed in m/s.</param>
    /// <param name="windDirection">Wind direction in degrees (0-360).</param>
    /// <returns>Score from -10 to +10.</returns>
    /// <remarks>
    /// Light to moderate wind (2-5 m/s): +8 to +10 (creates surface action, oxygenates water)
    /// Calm (0-2 m/s): +2 to +5 (neutral to slightly positive)
    /// Strong wind (5-10 m/s): 0 to -5 (difficult conditions)
    /// Very strong wind (&gt;10 m/s): -5 to -10 (poor conditions)
    /// Direction bonus: onshore winds often better than offshore
    /// </remarks>
    private static double CalculateWindScore(double windSpeed, double windDirection)
    {
        double score;

        // Wind speed scoring
        if (windSpeed >= 2 && windSpeed <= 5)
            score = 10; // Ideal wind: creates ripples, oxygenates, masks approach
        else if (windSpeed < 2)
            score = 3; // Calm: neutral to slightly positive
        else if (windSpeed <= 8)
            score = 5 - (windSpeed - 5) * 1.67; // Moderate to strong: decreasing benefit
        else if (windSpeed <= 12)
            score = -5; // Strong wind: difficult
        else
            score = -10; // Very strong wind: poor conditions

        // Direction bonus (simplified: 0-180 is roughly onshore for many locations)
        // This is a general approximation; specific locations may vary
        if (windDirection >= 45 && windDirection <= 135)
            score += 2; // Slight bonus for certain directions

        return Math.Clamp(score, -10, 10);
    }

    /// <summary>
    /// Calculates the cloud cover modifier.
    /// </summary>
    /// <param name="cloudCover">Cloud cover percentage (0-100).</param>
    /// <returns>Score from -5 to +10.</returns>
    /// <remarks>
    /// Overcast (70-100%): +8 to +10 (fish feel secure, feed actively)
    /// Partly cloudy (30-70%): +5 to +8 (good conditions)
    /// Clear skies (0-30%): 0 to +3 (fish more cautious, especially in shallow water)
    /// </remarks>
    private static double CalculateCloudCoverScore(double cloudCover)
    {
        if (cloudCover >= 70)
            return 10; // Overcast: excellent (fish less wary)
        if (cloudCover >= 30)
            return 5 + (cloudCover - 30) * 0.125; // Partly cloudy: good, scaling to excellent
        
        // Clear skies: neutral to slightly positive
        return cloudCover * 0.1; // 0-3 points
    }

    /// <summary>
    /// Calculates the wave/current modifier.
    /// </summary>
    /// <param name="waveHeight">Wave height in meters.</param>
    /// <param name="currentSpeed">Current speed in m/s.</param>
    /// <returns>Score from -10 to +10.</returns>
    /// <remarks>
    /// Moderate waves (0.3-1.0m) + moderate current (0.1-0.5 m/s): +8 to +10 (excellent)
    /// Light action: +3 to +5 (good)
    /// Calm: 0 to +2 (neutral)
    /// Very rough (&gt;2m waves): -5 to -10 (difficult, fish seek shelter)
    /// </remarks>
    private static double CalculateWaveScore(double waveHeight, double currentSpeed)
    {
        double score = 0;

        // Wave height contribution
        if (waveHeight >= 0.3 && waveHeight <= 1.0)
            score += 5; // Moderate waves: good
        else if (waveHeight < 0.3)
            score += 2; // Calm: slightly positive
        else if (waveHeight <= 2.0)
            score -= (waveHeight - 1.0) * 5; // Increasing waves: decreasing benefit
        else
            score -= 10; // Very rough: poor

        // Current speed contribution
        if (currentSpeed >= 0.1 && currentSpeed <= 0.5)
            score += 5; // Moderate current: excellent (brings food, oxygenates)
        else if (currentSpeed < 0.1)
            score += 1; // Weak current: slightly positive
        else if (currentSpeed <= 1.0)
            score += 5 - (currentSpeed - 0.5) * 10; // Strong current: decreasing benefit
        else
            score -= 5; // Very strong current: difficult

        return Math.Clamp(score, -10, 10);
    }

    /// <summary>
    /// Calculates the air temperature modifier.
    /// </summary>
    /// <param name="airTemp">Air temperature in Celsius.</param>
    /// <returns>Score from -5 to +5.</returns>
    /// <remarks>
    /// Comfortable range (15-25°C): +3 to +5 (good surface activity)
    /// Cool (10-15°C or 25-30°C): 0 to +3 (acceptable)
    /// Extreme (&lt;5°C or &gt;35°C): -3 to -5 (reduced surface activity)
    /// </remarks>
    private static double CalculateAirTemperatureScore(double airTemp)
    {
        if (airTemp >= 15 && airTemp <= 25)
            return 5; // Comfortable range
        if (airTemp >= 10 && airTemp < 15)
            return (airTemp - 10); // 0 to +5
        if (airTemp > 25 && airTemp <= 30)
            return 5 - (airTemp - 25); // +5 to 0
        if (airTemp >= 5 && airTemp < 10)
            return -5 + (airTemp - 5); // -5 to 0
        if (airTemp > 30 && airTemp <= 35)
            return -(airTemp - 30); // 0 to -5

        // Extreme temperatures
        return airTemp < 5 ? -5 : -5;
    }

    /// <summary>
    /// Calculates the humidity modifier.
    /// </summary>
    /// <param name="humidity">Relative humidity percentage (0-100).</param>
    /// <returns>Score from -3 to +3.</returns>
    /// <remarks>
    /// Moderate to high humidity (60-80%): +2 to +3 (indicates stable weather)
    /// Normal humidity (40-60%): 0 to +2 (neutral)
    /// Very low or very high humidity: -1 to -3 (can indicate weather extremes)
    /// </remarks>
    private static double CalculateHumidityScore(double humidity)
    {
        if (humidity >= 60 && humidity <= 80)
            return 3; // Ideal range
        if (humidity >= 40 && humidity < 60)
            return (humidity - 40) * 0.1; // 0 to +2
        if (humidity > 80 && humidity <= 90)
            return 3 - (humidity - 80) * 0.4; // +3 to -1
        
        // Very low or very high humidity
        return humidity < 40 ? -2 : -3;
    }
}
