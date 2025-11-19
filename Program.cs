using System.Globalization;
using System.Text.Json;
using SolunarBase.Models;
using SolunarBase.Services;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace SolunarBase
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Load appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .Build();

            var settings = new SolunarSettings();
            config.GetSection("SolunarSettings").Bind(settings);

            // Args: lat lon date(yyyy-MM-dd) [timeZoneId]
            // Command line overrides appsettings
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

            var astro = new AstronomyCalculator();
            var solunar = new SolunarCalculator(astro);

            var input = new SolunarInput
            {
                Latitude = lat,
                Longitude = lon,
                Date = date,
                TimeZoneId = timeZoneId
            };

            var result = solunar.Calculate(input);

            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
            Console.WriteLine(json);

            // Save to Output directory
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            string latStr = (args.Length >= 2 ? args[0] : lat.ToString("G17", CultureInfo.InvariantCulture)).Replace(',', '.');
            string lonStr = (args.Length >= 2 ? args[1] : lon.ToString("G17", CultureInfo.InvariantCulture)).Replace(',', '.');
            string dateStr = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            string fileName = $"solunar_{latStr}_{lonStr}_{dateStr}.json";
            string filePath = Path.Combine(outputDir, fileName);
            File.WriteAllText(filePath, json);
            Console.WriteLine($"Saved to {filePath}");
        }
    }
}
