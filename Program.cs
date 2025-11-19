using System.Globalization;
using System.Text.Json;
using SolunarBase.Models;
using SolunarBase.Services;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace SolunarBase
{
    /// <summary>
    /// Console entry point that orchestrates the solunar calculation workflow.
    /// Steps:
    /// 1. Load configuration (appsettings + defaults)
    /// 2. Parse command-line overrides (lat, lon, date, timezone)
    /// 3. Build input model
    /// 4. Perform solunar calculations
    /// 5. Serialize result to JSON and write to console
    /// 6. Ensure output directory exists
    /// 7. Persist result JSON to file
    /// 8. Report saved file path
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            // STEP 1: Load configuration (appsettings.json) into a settings object
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .Build();

            var settings = new SolunarSettings();
            config.GetSection("SolunarSettings").Bind(settings);

            // STEP 2: Parse command-line overrides (Args: lat lon date(yyyy-MM-dd) [timeZoneId])
            // If provided, these supersede values from configuration
            double lat = settings.Latitude;
            double lon = settings.Longitude;
            DateOnly date = DateOnly.FromDateTime(DateTime.UtcNow);
            string timeZoneId = settings.TimeZoneId;
            string outputDir = settings.OutputDirectory;

            if (args.Length >= 2)
            {
                double.TryParse(args[0], NumberStyles.Float, CultureInfo.InvariantCulture, out lat);
                double.TryParse(args[1], NumberStyles.Float, CultureInfo.InvariantCulture, out lon);
            }
            if (args.Length >= 3)
            {
                if (DateOnly.TryParse(args[2], CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                {
                    date = d;
                }
            }
            if (args.Length >= 4)
            {
                timeZoneId = args[3];
            }

            // STEP 3: Instantiate calculators (astronomy + solunar logic)
            var astro = new AstronomyCalculator();
            var solunar = new SolunarCalculator(astro);

            // STEP 4: Build the input DTO with resolved parameters
            var input = new SolunarInput
            {
                Latitude = lat,
                Longitude = lon,
                Date = date,
                TimeZoneId = timeZoneId
            };

            // STEP 5: Execute solunar calculation producing all periods & scores
            var result = solunar.Calculate(input);

            // STEP 6: Serialize result to JSON (camelCase, ignore nulls) and print to console
            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
            Console.WriteLine(json);

            // STEP 7: Ensure output directory exists (create if missing)
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // STEP 8: Construct deterministic filename (lat_lon_date) and persist JSON
            string latStr = (args.Length >= 2 ? args[0] : lat.ToString("G17", CultureInfo.InvariantCulture)).Replace(',', '.');
            string lonStr = (args.Length >= 2 ? args[1] : lon.ToString("G17", CultureInfo.InvariantCulture)).Replace(',', '.');
            string dateStr = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            string fileName = $"solunar_{latStr}_{lonStr}_{dateStr}.json";
            string filePath = Path.Combine(outputDir, fileName);
            File.WriteAllText(filePath, json);
            Console.WriteLine($"Saved to {filePath}"); // Final status output
        }
    }
}
