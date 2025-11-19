namespace SolunarBase.Utils;

public static class DateTimeExtensions
{
    public static DateTime ToUtcStartOfDay(this DateOnly date)
    {
        return date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
    }

    public static DateTime ToUtcEndOfDay(this DateOnly date)
    {
        return date.ToDateTime(new TimeOnly(23, 59, 59), DateTimeKind.Utc);
    }

    public static DateTime AtUtc(this DateOnly date, int hour, int minute = 0)
    {
        return date.ToDateTime(new TimeOnly(hour, minute), DateTimeKind.Utc);
    }
}
