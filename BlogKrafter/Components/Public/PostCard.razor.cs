namespace BlogKrafter.Components.Public;

/// <summary>One post as a list card: cover, title, excerpt, and the date line.</summary>
public partial class PostCard
{
    /// <summary>The post the card shows.</summary>
    [Parameter, EditorRequired]
    public PostSummaryDto Post { get; set; } = default!;

    [Inject]
    private BlogUrls Urls { get; set; } = default!;
}