namespace BlogKrafter.Components;

/// <summary>Used when the host has not set a public layout, so the blog still reads as a page of its own.</summary>
public partial class BlogDefaultLayout
{
    [Inject]
    private IOptions<BlogKrafterOptions> Options { get; set; } = default!;

    [Inject]
    private BlogUrls Urls { get; set; } = default!;
}