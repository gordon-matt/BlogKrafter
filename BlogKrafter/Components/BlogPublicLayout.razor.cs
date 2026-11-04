namespace BlogKrafter.Components;

/// <summary>
/// The layout of the public pages: the host's <see cref="BlogKrafterOptions.PublicLayout"/> when it set
/// one, otherwise a plain page with the blog's title.
/// </summary>
public partial class BlogPublicLayout
{
    [Inject]
    private IOptions<BlogKrafterOptions> Options { get; set; } = default!;

    private Type Layout => Options.Value.PublicLayout ?? typeof(BlogDefaultLayout);
}