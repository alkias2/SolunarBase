namespace SolunarBase.Utils;

public static class TimeZoneHelper
{
    public static TimeZoneInfo Resolve(string? timeZoneId)
    {
        // Default to Europe/Athens if specified; otherwise UTC
        var id = string.IsNullOrWhiteSpace(timeZoneId) ? "Europe/Athens" : timeZoneId!;

        // Try direct lookup (works on Linux/macOS for IANA, Windows for Windows IDs)
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch
        {
            // Minimal mapping for common IANA -> Windows on Windows hosts
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Europe/Athens", "GTB Standard Time" },
                { "UTC", "UTC" }
            };
            if (map.TryGetValue(id, out var windowsId))
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById(windowsId); } catch { }
            }

            // Fallback to UTC
            return TimeZoneInfo.Utc;
        }
    }

    public static DateTime ToLocal(DateTime utc, TimeZoneInfo tz)
    {
        if (utc.Kind != DateTimeKind.Utc) utc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
    }

    public static DateTime LocalMidHourToUtc(DateOnly localDate, int hour, TimeZoneInfo tz)
    {
        var local = localDate.ToDateTime(new TimeOnly(hour, 30));
        local = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(local, tz);
    }
}
