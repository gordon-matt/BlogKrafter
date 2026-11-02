namespace BlogKrafter.Infrastructure;

/// <summary>
/// Converts between stored UTC and the editor's own clock, so a publish date is entered and shown in
/// the author's time zone. Falls back to UTC until the browser has said where it is.
/// </summary>
internal sealed class BrowserTime(TimeZoneInfo zone)
{
    public static BrowserTime Utc { get; } = new(TimeZoneInfo.Utc);

    public bool IsUtc => zone == TimeZoneInfo.Utc;

    public string ZoneName => IsUtc ? "UTC" : zone.Id;

    public static BrowserTime From(string? ianaId)
    {
        if (!string.IsNullOrWhiteSpace(ianaId) && TimeZoneInfo.TryFindSystemTimeZoneById(ianaId, out var zone))
        {
            return new BrowserTime(zone);
        }

        return Utc;
    }

    public DateTime ToLocal(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), zone);

    public DateTime ToUtc(DateTime local)
    {
        var unspecified = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);

        // A wall-clock time skipped by a daylight-saving change does not exist; read it as the hour after.
        if (zone.IsInvalidTime(unspecified))
        {
            unspecified = unspecified.AddHours(1);
        }

        return TimeZoneInfo.ConvertTimeToUtc(unspecified, zone);
    }
}
