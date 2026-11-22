namespace BlogKrafter.Components.Admin;

/// <summary>Links between the posts, categories, and tags screens.</summary>
public partial class AdminNav
{
    /// <summary>The screen to highlight. <see langword="null"/> highlights none.</summary>
    [Parameter]
    public AdminSection? Current { get; set; }

    [Inject]
    private BlogUrls Urls { get; set; } = default!;

    private string ButtonClass(AdminSection section) =>
        Current == section ? "nf-button nf-button-primary" : "nf-button";

    private string? AriaCurrent(AdminSection section) => Current == section ? "page" : null;
}