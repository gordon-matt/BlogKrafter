namespace BlogKrafter.Components.Public;

/// <summary>
/// The blog at its default address. A host that moves the blog renders <see cref="BlogView"/> from a page
/// with its own routes; this one then answers 404 so the old address does not serve a second copy.
/// </summary>
public partial class BlogPage
{
    private bool isDefaultPath;

    [Parameter]
    public string? Slug { get; set; }

    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public string? Category { get; set; }

    [Inject]
    private BlogUrls Urls { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    protected override void OnInitialized()
    {
        isDefaultPath = string.Equals(Urls.BasePath, BlogKrafterOptions.DefaultBasePath, StringComparison.OrdinalIgnoreCase);
        if (!isDefaultPath)
        {
            Navigation.NotFound();
        }
    }
}
