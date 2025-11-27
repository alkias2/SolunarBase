using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using SolunarBase.Models;
using SolunarBase.Services;

namespace SolunarBase
{
    /// <summary>
    /// Console entry point that orchestrates the solunar calculation workflow.
    /// Steps:
    /// 1. Load configuration (appsettings + defaults)
    /// 2. Parse command-line overrides (lat, lon, date, timezone)
    /// 3. Load optional Weather and Tide JSON data files
    /// 4. Build input model with all available data
    /// 5. Perform solunar calculations with modifiers
    /// 6. Serialize result to JSON and write to console
    /// 7. Ensure output directory exists
    /// 8. Persist result JSON to file
    /// 9. Report saved file path
    /// </summary>
    internal class Program
    {
        private static void Main(string[] args) {
            // STEP 1: Load configuration (appsettings.json) into a settings object
            // Resolve project root regardless of current working directory (VS vs VS Code)
            var exeBase = AppContext.BaseDirectory; // ...\bin\Debug\net8.0\
            var projectRoot = Path.GetFullPath(Path.Combine(exeBase, "..", "..", ".."));

            var config = new ConfigurationBuilder()
                .SetBasePath(projectRoot)
                .AddJsonFile("appsettings.json", true, false)
                .Build();

            var settings = new SolunarSettings();
            config.GetSection("SolunarSettings").Bind(settings);

            // STEP 2: Parse command-line overrides (Args: lat lon date(yyyy-MM-dd) [timeZoneId])
            // If provided, these supersede values from configuration
            double lat = settings.Latitude;
            double lon = settings.Longitude;
            // Resolve date: command-line > appsettings > today (UTC)
            DateOnly date = DateOnly.FromDateTime(DateTime.UtcNow);
            if (!string.IsNullOrWhiteSpace(settings.Date)) {
                if (DateOnly.TryParseExact(settings.Date.Trim(),
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var cfgDate)) {
                    date = cfgDate;
                }
            }

            string timeZoneId = settings.TimeZoneId;
            // Resolve output directory relative to project root if not absolute
            string outputDir = settings.OutputDirectory;
            if (!Path.IsPathRooted(outputDir)) {
                outputDir = Path.GetFullPath(Path.Combine(projectRoot, outputDir));
            }

            if (args.Length >= 2) {
                double.TryParse(args[0], NumberStyles.Float, CultureInfo.InvariantCulture, out lat);
                double.TryParse(args[1], NumberStyles.Float, CultureInfo.InvariantCulture, out lon);
            }

            if (args.Length >= 3) {
                if (DateOnly.TryParse(args[2], CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)) {
                    date = d;
                }
            }

            if (args.Length >= 4) {
                timeZoneId = args[3];
            }

            // STEP 3: Load optional Weather and Tide data from JSON files in settings.InputDirectory
            List<WeatherData>? weatherData = null;
            List<TideData>? tideData = null;
            ModifierWeights? weights = null;

            // Resolve input directory relative to project root if not absolute
            string referenceDir = settings.InputDirectory ?? "_ReferenceFiles-Solunar";
            if (!Path.IsPathRooted(referenceDir)) {
                referenceDir = Path.GetFullPath(Path.Combine(projectRoot, referenceDir));
            }

            if (!Directory.Exists(referenceDir)) {
                // Fallback to legacy default directory under project root
                var fallback = Path.GetFullPath(Path.Combine(projectRoot, "_ReferenceFiles-Solunar"));
                if (Directory.Exists(fallback)) {
                    referenceDir = fallback;
                }
            }

            // Try to load Weather.json
            string weatherFile = Path.Combine(referenceDir, settings.WeatherFile ?? "Weather.json");
            if (File.Exists(weatherFile)) {
                try {
                    string weatherJson = File.ReadAllText(weatherFile);
                    var weatherCollection = JsonSerializer.Deserialize<WeatherDataCollection>(weatherJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    weatherData = weatherCollection?.Weather;
                    if (weatherData != null && weatherData.Count > 0) {
                        Console.WriteLine($"Loaded {weatherData.Count} weather observations from Weather.json");
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine($"Warning: Could not load Weather.json: {ex.Message}");
                }
            }

            // Try to load Tide.json
            string tideFile = Path.Combine(referenceDir, settings.TideFile ?? "Tide.json");
            if (File.Exists(tideFile)) {
                try {
                    string tideJson = File.ReadAllText(tideFile);
                    var tideCollection = JsonSerializer.Deserialize<TideDataCollection>(tideJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    tideData = tideCollection?.Tide;
                    if (tideData != null && tideData.Count > 0) {
                        Console.WriteLine($"Loaded {tideData.Count} tide events from Tide.json");
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine($"Warning: Could not load Tide.json: {ex.Message}");
                }
            }

            // Load weights from appsettings or use defaults from ModifierWeights class
            if (settings.WeightSettings != null) {
                weights = settings.WeightSettings;
                Console.WriteLine("Loaded custom modifier weights from appsettings.json");
            }
            else {
                Console.WriteLine("Using default modifier weights from ModifierWeights class");
            }

            // STEP 4: Instantiate calculators (astronomy + solunar logic)
            var astro = new AstronomyCalculator();
            var solunar = new SolunarCalculator(astro);

            // STEP 5: Build the input DTO with resolved parameters and optional modifiers
            var input = new SolunarInput {
                Latitude = lat,
                Longitude = lon,
                Date = date,
                TimeZoneId = timeZoneId,
                WeatherData = weatherData,
                TideData = tideData,
                Weights = weights
            };

            // STEP 6: Execute solunar calculation producing all periods & scores with modifiers
            var result = solunar.Calculate(input);

            // STEP 7: Serialize result to JSON (camelCase, ignore nulls) and print to console
            var json = JsonSerializer.Serialize(result,
                new JsonSerializerOptions {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });
            Console.WriteLine(json);

            // STEP 8: Ensure output directory exists (create if missing)
            if (!Directory.Exists(outputDir)) {
                Directory.CreateDirectory(outputDir);
            }

            // STEP 9: Construct deterministic filename (lat_lon_date) and persist JSON
            string latStr = (args.Length >= 2 ? args[0] : lat.ToString("G17", CultureInfo.InvariantCulture)).Replace(',', '.');
            string lonStr = (args.Length >= 2 ? args[1] : lon.ToString("G17", CultureInfo.InvariantCulture)).Replace(',', '.');
            string dateStr = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            string fileName = $"solunar_{latStr}_{lonStr}_{dateStr}.json";
            string filePath = Path.Combine(outputDir, fileName);
            File.WriteAllText(filePath, json);
            Console.WriteLine($"Saved JSON to {filePath}"); // JSON output status

            // STEP 10: Export to CSV files for Excel analysis
            try {
                // Export major/minor periods to CSV
                string periodsFile = Path.Combine(outputDir, $"solunar_periods_{latStr}_{lonStr}_{dateStr}.csv");
                CsvExporter.ExportPeriodsToCsv(result, periodsFile);
                Console.WriteLine($"Saved Periods CSV to {periodsFile}");

                // Export hourly activity to CSV
                string hourlyFile = Path.Combine(outputDir, $"solunar_hourly_{latStr}_{lonStr}_{dateStr}.csv");
                CsvExporter.ExportHourlyActivityToCsv(result, hourlyFile);
                Console.WriteLine($"Saved Hourly Activity CSV to {hourlyFile}");

                // Export summary to CSV
                string summaryFile = Path.Combine(outputDir, $"solunar_summary_{latStr}_{lonStr}_{dateStr}.csv");
                CsvExporter.ExportSummaryToCsv(result, summaryFile);
                Console.WriteLine($"Saved Summary CSV to {summaryFile}");
            }
            catch (Exception ex) {
                Console.WriteLine($"Warning: Error exporting CSV files: {ex.Message}");
            }

            // STEP 11: Copy solunar.json to _ReferenceFiles-Solunar for web visualization
            try {
                string visualizationDir = Path.GetFullPath(Path.Combine(projectRoot, "Output"));
                if (Directory.Exists(visualizationDir)) {
                    string vizJsonPath = Path.Combine(visualizationDir, "solunar.json");
                    File.WriteAllText(vizJsonPath, json);
                    Console.WriteLine($"Copied solunar.json to {vizJsonPath} for web visualization");
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Warning: Error exporting CSV files: {ex.Message}");
            }
        }
    }
}