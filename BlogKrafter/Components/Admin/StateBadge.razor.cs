namespace BlogKrafter.Components.Admin;

/// <summary>A badge reading Draft, Scheduled, or Published.</summary>
public partial class StateBadge
{
    /// <summary>The state the badge shows.</summary>
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