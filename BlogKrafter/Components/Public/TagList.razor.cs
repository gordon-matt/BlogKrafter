namespace BlogKrafter.Components.Public;

public partial class TagList
{
    [Parameter]
    public IReadOnlyList<TagRef> Tags { get; set; } = [];

    [Inject]
    private BlogUrls Urls { get; set; } = default!;
}