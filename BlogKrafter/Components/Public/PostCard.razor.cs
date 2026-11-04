namespace BlogKrafter.Components.Public;

public partial class PostCard
{
    [Parameter, EditorRequired]
    public PostSummaryDto Post { get; set; } = default!;

    [Inject]
    private BlogUrls Urls { get; set; } = default!;
}