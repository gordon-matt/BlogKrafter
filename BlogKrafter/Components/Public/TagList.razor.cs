namespace BlogKrafter.Components.Public;

/// <summary>
/// Represents the tag list.
/// </summary>
public partial class TagList
{
    /// <summary>
    /// Gets or sets the tags.
    /// </summary>
    [Parameter]
    public IReadOnlyList<TagRef> Tags { get; set; } = [];

    [Inject]
    private BlogUrls Urls { get; set; } = default!;
}