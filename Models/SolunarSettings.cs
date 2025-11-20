namespace SolunarBase.Models;

public class SolunarSettings
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string TimeZoneId { get; set; } = "Europe/Athens";
    public string OutputDirectory { get; set; } = "Output";

    /// <summary>
    /// Optional ISO date string (yyyy-MM-dd). If provided, overrides current date.
    /// </summary>
    public string? Date { get; set; }

    /// <summary>
    /// Directory to read input JSON files (Weather/Tide/etc.).
    /// Relative paths are resolved against the working directory.
    /// </summary>
    public string InputDirectory { get; set; } = "_ReferenceFiles-Solunar";

    /// <summary>
    /// Weather data filename (inside InputDirectory).
    /// </summary>
    public string WeatherFile { get; set; } = "Weather.json";

    /// <summary>
    /// Tide data filename (inside InputDirectory).
    /// </summary>
    public string TideFile { get; set; } = "Tide.json";

    /// <summary>
    /// Modifier weights configuration. If not provided in appsettings, defaults from ModifierWeights class are used.
    /// </summary>
    public ModifierWeights? WeightSettings { get; set; }
}
