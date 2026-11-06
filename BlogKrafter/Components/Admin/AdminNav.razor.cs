namespace BlogKrafter.Components.Admin;

/// <summary>
/// Represents the admin nav.
/// </summary>
public partial class AdminNav
{
    /// <summary>
    /// Gets or sets the current.
    /// </summary>
    [Parameter]
    public AdminSection? Current { get; set; }

    [Inject]
    private BlogUrls Urls { get; set; } = default!;

    private string ButtonClass(AdminSection section) =>
        Current == section ? "nf-button nf-button-primary" : "nf-button";

    private string? AriaCurrent(AdminSection section) => Current == section ? "page" : null;
}