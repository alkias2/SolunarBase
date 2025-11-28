namespace SolunarBase.Models;

public enum SolunarPeriodType
{
    LowerTransit,  // 0 - Underfoot (occurs first chronologically in most cases)
    UpperTransit,  // 1 - Overhead (occurs second chronologically in most cases)
    Moonrise,      // 2
    Moonset        // 3
}

public class SolunarPeriod
{
    public SolunarPeriodType Type { get; set; }
    public DateTime Start { get; set; } // UTC
    public DateTime End { get; set; }   // UTC
    public DateTime Center { get; set; } // UTC
}
