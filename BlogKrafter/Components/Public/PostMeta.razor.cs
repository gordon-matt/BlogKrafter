using System.Globalization;

namespace BlogKrafter.Components.Public;

/// <summary>The line of date, author, reading time and category above a post or a card.</summary>
public partial class PostMeta
{
    /// <summary>The publish time in UTC. It is shown in the blog's time zone. <see langword="null"/> omits the date.</summary>
    [Parameter]
    public DateTime? PublishedAt { get; set; }

    /// <summary>The author's name. Blank omits it.</summary>
    [Parameter]
    public string? AuthorName { get; set; }

    /// <summary>Minutes to read. Zero omits the reading time.</summary>
    [Parameter]
    public int ReadingTimeMinutes { get; set; }

    /// <summary>The category link. <see langword="null"/> omits it.</summary>
    [Parameter]
    public CategoryRef? Category { get; set; }

    [Inject]
    private BlogUrls Urls { get; set; } = default!;

    [Inject]
    private IOptions<BlogKrafterOptions> Options { get; set; } = default!;

    [Inject]
    private BlogTimeZone TimeZone { get; set; } = default!;

    private DateTime? LocalPublishedAt => PublishedAt is DateTime published ? TimeZone.ToLocal(published) : null;

    private string FormatDate(DateTime value) => value.ToString(Options.Value.DateFormat, CultureInfo.CurrentCulture);
}