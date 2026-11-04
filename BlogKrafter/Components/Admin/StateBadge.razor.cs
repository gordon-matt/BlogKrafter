namespace BlogKrafter.Components.Admin;

public partial class StateBadge
{
    [Parameter]
    public PostState State { get; set; }

    private string Label => State switch
    {
        PostState.Published => "Published",
        PostState.Scheduled => "Scheduled",
        _ => "Draft",
    };

    private string CssClass => State switch
    {
        PostState.Published => "nf-badge-success",
        PostState.Scheduled => "nf-badge-info",
        _ => "nf-badge-muted",
    };
}