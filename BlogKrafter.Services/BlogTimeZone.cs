namespace BlogKrafter;

/// <summary>
/// Converts stored UTC times to <see cref="BlogKrafterOptions.TimeZone"/> for display, so every public
/// page, and the archive's grouping by year, agrees on which day a post went live.
/// </summary>
public sealed class BlogTimeZone(IOptions<BlogKrafterOptions> options)
{
    private readonly TimeZoneInfo zone = string.IsNullOrWhiteSpace(options.Value.TimeZone)
        ? TimeZoneInfo.Utc
        : TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZone);

    /// <summary>
    /// Converts the value to the local.
    /// </summary>
    /// <param name="utc">The utc.</param>
    /// <returns>The local.</returns>
    public DateTime ToLocal(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), zone);
}