namespace BlogKrafter.Components.Public;

/// <summary>Links to the tags named on a post.</summary>
public partial class TagList
{
    /// <summary>The tags to link. Empty shows nothing.</summary>
    [Parameter]
    public IReadOnlyList<TagRef> Tags { get; set; } = [];

    [Inject]
    private BlogUrls Urls { get; set; } = default!;
}