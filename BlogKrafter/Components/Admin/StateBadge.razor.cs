namespace BlogKrafter.Components.Admin;

/// <summary>
/// Represents the state badge.
/// </summary>
public partial class StateBadge
{
    /// <summary>
    /// Gets or sets the state.
    /// </summary>
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