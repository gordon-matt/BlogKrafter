namespace BlogKrafter.Components.Admin;

/// <summary>
/// The editor at its default address. A host that moves it renders <see cref="BlogAdminView"/> from a
/// page with its own routes; this one then answers 404.
/// </summary>
public partial class BlogAdminPage
{
    private bool isDefaultPath;

    [Parameter]
    public string? Section { get; set; }

    [Parameter]
    public string? Id { get; set; }

    [Inject]
    private BlogUrls Urls { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    protected override void OnInitialized()
    {
        isDefaultPath = string.Equals(Urls.AdminPath, BlogKrafterOptions.DefaultAdminPath, StringComparison.OrdinalIgnoreCase);
        if (!isDefaultPath)
        {
            Navigation.NotFound();
        }
    }
}