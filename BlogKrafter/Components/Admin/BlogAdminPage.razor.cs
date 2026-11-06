namespace BlogKrafter.Components.Admin;

/// <summary>
/// The editor at its default address. A host that moves it renders <see cref="BlogAdminView"/> from a
/// page with its own routes; this one then answers 404.
/// </summary>
public partial class BlogAdminPage
{
    private bool isDefaultPath;

    /// <summary>
    /// Gets or sets the section.
    /// </summary>
    [Parameter]
    public string? Section { get; set; }

    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    [Inject]
    private BlogUrls Urls { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        isDefaultPath = string.Equals(Urls.AdminPath, BlogKrafterOptions.DefaultAdminPath, StringComparison.OrdinalIgnoreCase);
        if (!isDefaultPath)
        {
            Navigation.NotFound();
        }
    }
}