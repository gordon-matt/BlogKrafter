using BlogKrafter.Posts;

namespace BlogKrafter.Components.Public;

/// <summary>
/// The latest posts, for a home page or a sidebar elsewhere on the site. Narrow it to a tag or a
/// category with their slugs. A failed read shows an empty list.
/// </summary>
public partial class RecentPosts
{
    private IReadOnlyList<PostSummaryDto>? posts;

    /// <summary>How many posts to show. The default is 3. Zero shows none.</summary>
    [Parameter]
    public int Count { get; set; } = 3;

    /// <summary>When set, only posts with this tag.</summary>
    [Parameter]
    public string? TagSlug { get; set; }

    /// <summary>When set, only posts in this category.</summary>
    [Parameter]
    public string? CategorySlug { get; set; }

    /// <summary>The heading. The default is "Latest posts". Blank omits it.</summary>
    [Parameter]
    public string? Title { get; set; } = "Latest posts";

    /// <summary>Titles and dates only, instead of cards.</summary>
    [Parameter]
    public bool Compact { get; set; }

    /// <summary>
    /// <see langword="true"/> shows a link to the tag, otherwise the category, otherwise the blog index.
    /// The default is <see langword="true"/>.
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