using SolunarBase.Models;

namespace SolunarBase.Services;

/// <summary>
/// Calculates tide-based activity modifiers for fish behavior.
/// </summary>
/// <remarks>
/// Tides are one of the strongest influences on fish feeding patterns, second only to solunar periods.
/// This calculator analyzes tide events and tidal movement to determine optimal fishing times.
/// Key principles:
/// - Incoming tide (approaching high): most active feeding
/// - Slack tide (no movement): lowest activity
/// - Strong tidal movement: increased activity
/// - Outgoing tide: decreasing activity
/// </remarks>
public class TideModifierCalculator
{
    private readonly ModifierWeights _weights;

    /// <summary>
    /// Initializes a new instance of the TideModifierCalculator.
    /// </summary>
    /// <param name="weights">The weighting configuration for tide factors.</param>
    public TideModifierCalculator(ModifierWeights weights)
    {
        _weights = weights;
    }

    /// <summary>
    /// Calculates the total tide modifier for a specific hour.
    /// </summary>
    /// <param name="evaluationTime">The time being evaluated.</param>
    /// <param name="tideEvents">All tide events (high and low tides) for the day.</param>
    /// <returns>A modifier score ranging approximately from -20 to +20 points.</returns>
    /// <remarks>
    /// The modifier is based on:
    /// 1. Distance from nearest tide event (high/low)
    /// 2. Direction of tidal movement (incoming vs outgoing)
    /// 3. Estimated tidal current strength
    /// Peak activity occurs 1-2 hours before high tide and during strong incoming tides.
    /// </remarks>
    public double CalculateModifier(DateTime evaluationTime, List<TideData> tideEvents)
    {
        if (tideEvents == null || tideEvents.Count == 0)
        {
            // No tide data available, return neutral modifier
            return 0;
        }

        // Sort tide events by timestamp
        var sortedTides = tideEvents.OrderBy(t => t.Timestamp).ToList();

        // Find the tides surrounding the evaluation time
        var (previousTide, nextTide) = FindSurroundingTides(evaluationTime, sortedTides);

        if (previousTide == null || nextTide == null)
        {
            // Can't determine tidal state without both surrounding tides
            return 0;
        }

        // Calculate tidal state and movement
        double levelScore = CalculateTideLevelScore(evaluationTime, previousTide, nextTide);
        double movementScore = CalculateTideMovementScore(evaluationTime, previousTide, nextTide);

        // Combine scores with weights
        double totalModifier = (levelScore * _weights.Tide.Level) + 
                              (movementScore * _weights.Tide.Movement);

        return Math.Clamp(totalModifier, -20, 20);
    }

    /// <summary>
    /// Finds the tide events immediately before and after the evaluation time.
    /// </summary>
    /// <param name="evaluationTime">The time being evaluated.</param>
    /// <param name="sortedTides">Tide events sorted by timestamp.</param>
    /// <returns>A tuple of (previous tide, next tide).</returns>
    private static (TideData? Previous, TideData? Next) FindSurroundingTides(
        DateTime evaluationTime, List<TideData> sortedTides)
    {
        TideData? previous = null;
        TideData? next = null;

        for (int i = 0; i < sortedTides.Count; i++)
        {
            if (sortedTides[i].Timestamp <= evaluationTime)
            {
                previous = sortedTides[i];
            }
            else
            {
                next = sortedTides[i];
                break;
            }
        }

        // If we didn't find a next tide in the list, check if we need to wrap
        // (evaluation time is after last tide)
        if (next == null && previous != null && sortedTides.Count > 0)
        {
            // Use the first tide of the next cycle (if available)
            // For now, return null to indicate we're past the last tide
        }

        // If we didn't find a previous tide, we're before the first tide
        // Use the previous day's last tide if needed (not implemented here)

        return (previous, next);
    }

    /// <summary>
    /// Calculates the tide level modifier based on whether tide is high, low, or in between.
    /// </summary>
    /// <param name="evaluationTime">The time being evaluated.</param>
    /// <param name="previousTide">The most recent tide event before evaluation time.</param>
    /// <param name="nextTide">The next tide event after evaluation time.</param>
    /// <returns>Score from -10 to +10.</returns>
    /// <remarks>
    /// Scoring:
    /// - 1-2 hours before high tide: +10 (peak feeding time)
    /// - At high tide: +8 (excellent)
    /// - 1-2 hours before low tide: +3 (moderate)
    /// - At low tide: -5 (reduced activity)
    /// - Slack tide (transition): -10 (minimal activity)
    /// </remarks>
    private static double CalculateTideLevelScore(DateTime evaluationTime, TideData previousTide, TideData nextTide)
    {
        // Calculate time position between tides (0.0 = at previous, 1.0 = at next)
        double totalMinutes = (nextTide.Timestamp - previousTide.Timestamp).TotalMinutes;
        double elapsedMinutes = (evaluationTime - previousTide.Timestamp).TotalMinutes;
        double position = totalMinutes > 0 ? elapsedMinutes / totalMinutes : 0.5;

        bool isIncoming = previousTide.TideType.ToLower() == "low" && nextTide.TideType.ToLower() == "high";
        bool isOutgoing = previousTide.TideType.ToLower() == "high" && nextTide.TideType.ToLower() == "low";

        if (isIncoming)
        {
            // Incoming tide: activity increases as we approach high tide
            // Best fishing is 1-2 hours before high tide (position 0.6-0.9)
            if (position >= 0.6 && position <= 0.9)
                return 10; // Peak activity window
            else if (position >= 0.3 && position < 0.6)
                return 5 + (position - 0.3) * 16.67; // Increasing from +5 to +10
            else if (position > 0.9)
                return 10 - (position - 0.9) * 20; // Slight decrease as we reach high tide
            else
                return position * 16.67; // Early incoming: 0 to +5
        }
        else if (isOutgoing)
        {
            // Outgoing tide: activity decreases as we approach low tide
            // Early outgoing can still be okay
            if (position < 0.2)
                return 8 - position * 15; // Just after high tide: +8 to +5
            else if (position < 0.5)
                return 5 - (position - 0.2) * 16.67; // Decreasing from +5 to 0
            else if (position < 0.8)
                return -(position - 0.5) * 16.67; // Continuing down to -5
            else
                return -5 - (position - 0.8) * 25; // Approaching low tide: -5 to -10
        }

        // Unknown tide pattern, return neutral
        return 0;
    }

    /// <summary>
    /// Calculates the tide movement modifier based on the strength of tidal current.
    /// </summary>
    /// <param name="evaluationTime">The time being evaluated.</param>
    /// <param name="previousTide">The most recent tide event before evaluation time.</param>
    /// <param name="nextTide">The next tide event after evaluation time.</param>
    /// <returns>Score from -10 to +10.</returns>
    /// <remarks>
    /// Scoring:
    /// - Strong tidal movement (mid-tide): +10 (peak activity - water moving, food dispersing)
    /// - Moderate movement: +5 to +8 (good)
    /// - Weak movement (near high or low): +2 to +5 (acceptable)
    /// - Slack tide (no movement): -10 (poor - stagnant water)
    /// Movement is strongest at the midpoint between high and low tides.
    /// </remarks>
    private static double CalculateTideMovementScore(DateTime evaluationTime, TideData previousTide, TideData nextTide)
    {
        // Calculate time position between tides (0.0 = at previous, 1.0 = at next)
        double totalMinutes = (nextTide.Timestamp - previousTide.Timestamp).TotalMinutes;
        double elapsedMinutes = (evaluationTime - previousTide.Timestamp).TotalMinutes;
        double position = totalMinutes > 0 ? elapsedMinutes / totalMinutes : 0.5;

        // Calculate height difference for current strength estimation
        double heightDifference = Math.Abs(nextTide.Height - previousTide.Height);

        // Movement is strongest at mid-tide (position ~0.5)
        // Use a parabolic function: movement = -4 * (position - 0.5)^2 + 1
        // This gives maximum at 0.5 and minimum at 0 and 1
        double movementFactor = 1 - 4 * Math.Pow(position - 0.5, 2);

        // Base score on movement factor
        double score;
        if (movementFactor > 0.8)
            score = 10; // Strong movement at mid-tide
        else if (movementFactor > 0.5)
            score = 5 + (movementFactor - 0.5) * 16.67; // +5 to +10
        else if (movementFactor > 0.2)
            score = (movementFactor - 0.2) * 16.67; // 0 to +5
        else
            score = -10 + movementFactor * 50; // -10 to 0 (approaching slack)

        // Adjust based on tide range (larger range = stronger currents)
        if (heightDifference > 1.0)
            score += 3; // Bonus for strong tidal range
        else if (heightDifference > 0.5)
            score += 1; // Small bonus for moderate range
        else if (heightDifference < 0.2)
            score -= 3; // Penalty for very weak tides

        return Math.Clamp(score, -10, 10);
    }
}
