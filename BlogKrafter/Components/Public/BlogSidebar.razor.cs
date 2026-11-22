using BlogKrafter.Posts;

namespace BlogKrafter.Components.Public;

/// <summary>Search, categories with posts, the most used tags, and the archive and feed links.</summary>
public partial class BlogSidebar
{
    private const int TagLimit = 30;

    private IReadOnlyList<CategoryDto> categories = [];
    private IReadOnlyList<TagDto> tags = [];

    /// <summary>The search the box shows. Blank leaves it empty.</summary>
    [Parameter]
    public string? Search { get; set; }

    /// <summary>The tag slug to highlight. <see langword="null"/> highlights none.</summary>
    [Parameter]
    public string? ActiveTag { get; set; }

    /// <summary>The category slug to highlight. <see langword="null"/> highlights none.</summary>
    [Parameter]
    public string? ActiveCategory { get; set; }

    [Inject]
    private IBlogReader Reader { get; set; } = default!;

    [Inject]
    private BlogUrls Urls { get; set; } = default!;

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        var foundCategories = await Reader.GetCategoriesAsync();
        categories = foundCategories.IsSuccess ? [.. foundCategories.Value.Where(category => category.PostCount > 0)] : [];

        var foundTags = await Reader.GetTagsAsync();
        tags = foundTags.IsSuccess ? [.. foundTags.Value.Take(TagLimit)] : [];
    }

    private static bool IsActive(string slug, string? active) =>
        string.Equals(slug, active, StringComparison.OrdinalIgnoreCase);
}