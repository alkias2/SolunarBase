namespace SolunarBase.Models;

public enum SolunarPeriodType
{
    UpperTransit,
    LowerTransit,
    Moonrise,
    Moonset
}

public class SolunarPeriod
{
    public SolunarPeriodType Type { get; set; }
    public DateTime Start { get; set; } // UTC
    public DateTime End { get; set; }   // UTC
    public DateTime Center { get; set; } // UTC
}
