namespace BlogKrafter.Components.Admin;

public partial class AdminNav
{
    [Parameter]
    public AdminSection? Current { get; set; }

    [Inject]
    private BlogUrls Urls { get; set; } = default!;

    private string ButtonClass(AdminSection section) =>
        Current == section ? "nf-button nf-button-primary" : "nf-button";

    private string? AriaCurrent(AdminSection section) => Current == section ? "page" : null;
}