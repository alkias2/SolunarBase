namespace SolunarBase.Models;

/// <summary>
/// Represents tidal event data (high tide, low tide).
/// </summary>
/// <remarks>
/// Tides are one of the strongest modifiers for fish activity after solunar periods.
/// Fish are typically most active during:
/// - Incoming tide (approaching high tide)
/// - Strong tidal movement
/// And least active during:
/// - Slack tide (no movement)
/// - Outgoing tide (approaching low tide)
/// Modifier range: -20 to +20
/// </remarks>
public class TideData
{
    /// <summary>
    /// Unique identifier for this tide event.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The timestamp when this tide event occurs.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Tide height in meters.
    /// </summary>
    /// <remarks>
    /// Positive values indicate height above mean sea level.
    /// Negative values indicate height below mean sea level.
    /// </remarks>
    public double Height { get; set; }

    /// <summary>
    /// Type of tide: "high" or "low".
    /// </summary>
    public string TideType { get; set; } = string.Empty;

    /// <summary>
    /// Name of the tidal station providing this data.
    /// </summary>
    public string StationName { get; set; } = string.Empty;

    /// <summary>
    /// Distance from the location to the tidal station (in kilometers).
    /// </summary>
    public string StationDistance { get; set; } = string.Empty;

    /// <summary>
    /// Data source identifier (e.g., "sg" for Stormglass).
    /// </summary>
    public string StationSource { get; set; } = string.Empty;
}

/// <summary>
/// Container for a collection of tide data records.
/// </summary>
public class TideDataCollection
{
    /// <summary>
    /// List of tidal events (high and low tides) for the day.
    /// </summary>
    public List<TideData> Tide { get; set; } = new();
}
