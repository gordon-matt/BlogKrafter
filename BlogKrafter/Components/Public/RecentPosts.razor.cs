using BlogKrafter.Posts;

namespace BlogKrafter.Components.Public;

/// <summary>
/// The latest posts, for a home page or a sidebar elsewhere on the site. Narrow it to a tag or a
/// category with their slugs.
/// </summary>
public partial class RecentPosts
{
    private IReadOnlyList<PostSummaryDto>? posts;

    /// <summary>
    /// Gets or sets the count.
    /// </summary>
    [Parameter]
    public int Count { get; set; } = 3;

    /// <summary>
    /// Gets or sets the tag slug.
    /// </summary>
    [Parameter]
    public string? TagSlug { get; set; }

    /// <summary>
    /// Gets or sets the category slug.
    /// </summary>
    [Parameter]
    public string? CategorySlug { get; set; }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    [Parameter]
    public string? Title { get; set; } = "Latest posts";

    /// <summary>Titles and dates only, instead of cards.</summary>
    [Parameter]
    public bool Compact { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is show more link.
    /// </summary>
    [Parameter]
    public bool ShowMoreLink { get; set; } = true;

    [Inject]
    private IBlogReader Reader { get; set; } = default!;

    [Inject]
    private IOptions<BlogKrafterOptions> Options { get; set; } = default!;

    [Inject]
    private BlogUrls Urls { get; set; } = default!;

    [Inject]
    private BlogTimeZone TimeZone { get; set; } = default!;

    private string MoreHref =>
        !string.IsNullOrWhiteSpace(TagSlug) ? Urls.Tag(TagSlug)
        : !string.IsNullOrWhiteSpace(CategorySlug) ? Urls.Category(CategorySlug)
        : Urls.Index();

    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync()
    {
        var found = await Reader.GetRecentPostsAsync(Count, TagSlug, CategorySlug);
        posts = found.IsSuccess ? found.Value : [];
    }
}