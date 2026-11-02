using System.Globalization;

namespace BlogKrafter.Components.Public;

/// <summary>The line of date, author, reading time and category above a post or a card.</summary>
public partial class PostMeta
{
    [Parameter]
    public DateTime? PublishedAt { get; set; }

    [Parameter]
    public string? AuthorName { get; set; }

    [Parameter]
    public int ReadingTimeMinutes { get; set; }

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
