using System.Globalization;
using System.Text;
using SolunarBase.Models;

namespace SolunarBase.Services;

/// <summary>
/// Exports solunar calculation results to CSV format for analysis in Excel or other tools.
/// </summary>
public static class CsvExporter
{
    /// <summary>
    /// Exports major and minor solunar periods to a CSV file.
    /// </summary>
    /// <param name="result">The solunar calculation result.</param>
    /// <param name="filePath">The path where the CSV file will be saved.</param>
    /// <remarks>
    /// CSV Format:
    /// Type, Start, End, Center, Duration (minutes)
    /// Includes both major times (lunar transits) and minor times (moonrise/moonset).
    /// </remarks>
    public static void ExportPeriodsToCsv(SolunarResult result, string filePath)
    {
        var csv = new StringBuilder();
        
        // CSV Header
        csv.AppendLine("Period Type,Start Time,End Time,Center Time,Duration (minutes)");

        // Export Major Times
        foreach (var period in result.MajorTimes)
        {
            string periodType = period.Type switch
            {
                SolunarPeriodType.UpperTransit => "Major - Upper Transit",
                SolunarPeriodType.LowerTransit => "Major - Lower Transit",
                _ => "Major"
            };

            double durationMinutes = (period.End - period.Start).TotalMinutes;

            csv.AppendLine($"{periodType}," +
                          $"{period.Start:yyyy-MM-dd HH:mm:ss}," +
                          $"{period.End:yyyy-MM-dd HH:mm:ss}," +
                          $"{period.Center:yyyy-MM-dd HH:mm:ss}," +
                          $"{durationMinutes:F0}");
        }

        // Export Minor Times
        foreach (var period in result.MinorTimes)
        {
            string periodType = period.Type switch
            {
                SolunarPeriodType.Moonrise => "Minor - Moonrise",
                SolunarPeriodType.Moonset => "Minor - Moonset",
                _ => "Minor"
            };

            double durationMinutes = (period.End - period.Start).TotalMinutes;

            csv.AppendLine($"{periodType}," +
                          $"{period.Start:yyyy-MM-dd HH:mm:ss}," +
                          $"{period.End:yyyy-MM-dd HH:mm:ss}," +
                          $"{period.Center:yyyy-MM-dd HH:mm:ss}," +
                          $"{durationMinutes:F0}");
        }

        File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
    }

    /// <summary>
    /// Exports hourly activity scores to a CSV file.
    /// </summary>
    /// <param name="result">The solunar calculation result.</param>
    /// <param name="filePath">The path where the CSV file will be saved.</param>
    /// <remarks>
    /// CSV Format:
    /// Hour, Activity Score, Solunar Score, Weather Modifier, Tide Modifier
    /// If no modifiers were used, only Hour and Activity Score are included.
    /// </remarks>
    public static void ExportHourlyActivityToCsv(SolunarResult result, string filePath)
    {
        var csv = new StringBuilder();
        
        // Check if we have breakdown data
        bool hasBreakdown = result.ActivityBreakdown != null && result.ActivityBreakdown.Count > 0;

        // CSV Header
        if (hasBreakdown)
        {
            csv.AppendLine("Hour,Time,Activity Score,Solunar Score,Weather Modifier,Tide Modifier,Total Components");
        }
        else
        {
            csv.AppendLine("Hour,Time,Activity Score");
        }

        // Export hourly data
        for (int i = 0; i < result.HourlyActivity.Count; i++)
        {
            var hourly = result.HourlyActivity[i];
            string time = $"{hourly.Hour:D2}:00";

            if (hasBreakdown && i < result.ActivityBreakdown!.Count)
            {
                var breakdown = result.ActivityBreakdown[i];
                double total = breakdown.SolunarScore + breakdown.WeatherModifier + breakdown.TideModifier;

                csv.AppendLine($"{hourly.Hour}," +
                              $"{time}," +
                              $"{hourly.Score}," +
                              $"{breakdown.SolunarScore:F2}," +
                              $"{breakdown.WeatherModifier:F2}," +
                              $"{breakdown.TideModifier:F2}," +
                              $"{total:F2}");
            }
            else
            {
                csv.AppendLine($"{hourly.Hour},{time},{hourly.Score}");
            }
        }

        File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
    }

    /// <summary>
    /// Exports a summary of the solunar calculation to a CSV file.
    /// </summary>
    /// <param name="result">The solunar calculation result.</param>
    /// <param name="filePath">The path where the CSV file will be saved.</param>
    /// <remarks>
    /// Includes date, location, moon phase, rating, and modifier flags.
    /// </remarks>
    public static void ExportSummaryToCsv(SolunarResult result, string filePath)
    {
        var csv = new StringBuilder();
        
        // CSV Header
        csv.AppendLine("Property,Value");

        // Basic information
        csv.AppendLine($"Date,{result.Date:yyyy-MM-dd}");
        csv.AppendLine($"Latitude,{result.Location.Latitude:F6}");
        csv.AppendLine($"Longitude,{result.Location.Longitude:F6}");
        csv.AppendLine($"Time Zone,{result.TimeZoneId}");
        
        // Moon phase
        csv.AppendLine($"Moon Phase,{result.MoonPhase.Phase}");
        csv.AppendLine($"Moon Illumination,{result.MoonPhase.Illumination:P1}");
        
        // Rating and modifiers
        csv.AppendLine($"Solunar Rating,{result.SolunarRating}");
        csv.AppendLine($"Has Weather Modifiers,{result.HasWeatherModifiers}");
        csv.AppendLine($"Has Tide Modifiers,{result.HasTideModifiers}");
        
        // Activity statistics
        int avgScore = (int)result.HourlyActivity.Average(h => h.Score);
        int maxScore = result.HourlyActivity.Max(h => h.Score);
        int minScore = result.HourlyActivity.Min(h => h.Score);
        var peakHours = result.HourlyActivity.Where(h => h.Score >= 80).Select(h => h.Hour).ToList();
        
        csv.AppendLine($"Average Activity Score,{avgScore}");
        csv.AppendLine($"Max Activity Score,{maxScore}");
        csv.AppendLine($"Min Activity Score,{minScore}");
        csv.AppendLine($"Peak Hours (>=80),\"{string.Join(", ", peakHours)}\"");
        
        // Period counts
        csv.AppendLine($"Major Periods,{result.MajorTimes.Count}");
        csv.AppendLine($"Minor Periods,{result.MinorTimes.Count}");

        File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
    }
}
