namespace TimeTracker.Web.Shared;

public static class TimeFormatHelper
{
    /// <summary>
    /// Formats a minute count as a human-readable string.
    /// Returns "—" for null, "Xm" when under 1 hour, "Xh 0Ym" (zero-padded) for 1 hour or more.
    /// </summary>
    public static string FormatMinutes(int? minutes)
    {
        if (minutes is null) return "—";

        var totalMinutes = minutes.Value;
        var hours = totalMinutes / 60;
        var mins = totalMinutes % 60;

        return hours == 0
            ? $"{mins}m"
            : $"{hours}h {mins:D2}m";
    }

    /// <inheritdoc cref="FormatMinutes(int?)"/>
    public static string FormatMinutes(int minutes) => FormatMinutes((int?)minutes);
}
