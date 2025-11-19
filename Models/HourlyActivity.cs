namespace SolunarBase.Models;

/// <summary>
/// Represents an activity score sample for a specific time slice during the day.
/// </summary>
/// <remarks>
/// Previously this model represented only hourly resolution (Hour 0-23).
/// It has been extended with a Minute component so the calculator can emit
/// 15-minute interval samples (96 points per day). The existing <see cref="Hour"/>
/// property is still populated (0-23) for backward compatibility, while <see cref="Minute"/>
/// holds the minute within the hour (0, 15, 30, 45).
/// </remarks>
public class HourlyActivity
{
    /// <summary>
    /// The hour of the day (0-23) in local time.
    /// </summary>
    public int Hour { get; set; }

    /// <summary>
    /// The minute component within the hour (typically 0, 15, 30, 45 for 15-min intervals).
    /// </summary>
    public int Minute { get; set; }

    /// <summary>
    /// Final clamped activity score (0-100) for this time slice.
    /// </summary>
    public int Score { get; set; }
}
