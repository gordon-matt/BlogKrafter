namespace BlogKrafter.Components.Admin;

public partial class AdminNav
{
    [Parameter]
    public AdminSection? Current { get; set; }

    [Inject]
    private BlogUrls Urls { get; set; } = default!;
}
