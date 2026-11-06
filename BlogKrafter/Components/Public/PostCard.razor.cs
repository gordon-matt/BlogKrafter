namespace BlogKrafter.Components.Public;

/// <summary>
/// Represents the post card.
/// </summary>
public partial class PostCard
{
    /// <summary>
    /// Gets or sets the post.
    /// </summary>
    [Parameter, EditorRequired]
    public PostSummaryDto Post { get; set; } = default!;

    [Inject]
    private BlogUrls Urls { get; set; } = default!;
}