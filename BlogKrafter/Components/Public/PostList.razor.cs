using BlogKrafter.Posts;
using Heroicsoft.NetForge.Results;

namespace BlogKrafter.Components.Public;

/// <summary>A page of published posts, optionally narrowed to a tag, a category or a search.</summary>
public partial class PostList
{
    private PostListResult? result;
    private string? loadError;

    [Parameter]
    public int Page { get; set; } = 1;

    [Parameter]
    public string? Search { get; set; }

    [Parameter]
    public string? TagSlug { get; set; }

    [Parameter]
    public string? CategorySlug { get; set; }

    [Inject]
    private IBlogReader Reader { get; set; } = default!;

    [Inject]
    private IOptions<BlogKrafterOptions> Options { get; set; } = default!;

    [Inject]
    private BlogUrls Urls { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private string PageTitleText
    {
        get
        {
            string title = Options.Value.Title;
            string scope = result?.Tag?.Name ?? result?.Category?.Name ?? string.Empty;
            string page = Page > 1 ? $" · Page {Page}" : string.Empty;
            return scope.Length > 0 ? $"{scope}{page} · {title}" : $"{title}{page}";
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        loadError = null;
        var found = await Reader.ListPostsAsync(new PostListQuery
        {
            Page = Math.Max(1, Page),
            Search = Search,
            TagSlug = TagSlug,
            CategorySlug = CategorySlug,
        });

        if (found.Status == ResultStatus.NotFound)
        {
            result = null;
            Navigation.NotFound();
            return;
        }

        result = found.IsSuccess ? found.Value : null;
        loadError = found.IsSuccess ? null : found.GetDisplayMessage();
    }

    private string PageHref(int page) => !string.IsNullOrWhiteSpace(TagSlug)
            ? Urls.Tag(TagSlug, page)
            : !string.IsNullOrWhiteSpace(CategorySlug)
            ? Urls.Category(CategorySlug, page)
            : Urls.Index(page, Search);

    private static string PostCountText(int count) => count == 1 ? "1 post" : $"{count:N0} posts";
}