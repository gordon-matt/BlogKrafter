using BlogKrafter.Posts;
using Heroicsoft.NetForge.Results;

namespace BlogKrafter.Components.Public;

public partial class ArchiveView
{
    private IReadOnlyList<ArchiveYear>? years;
    private string? loadError;

    [Inject]
    private IBlogReader Reader { get; set; } = default!;

    [Inject]
    private IOptions<BlogKrafterOptions> Options { get; set; } = default!;

    [Inject]
    private BlogUrls Urls { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        var found = await Reader.GetArchiveAsync();
        years = found.IsSuccess ? found.Value : null;
        loadError = found.IsSuccess ? null : found.GetDisplayMessage();
    }
}
