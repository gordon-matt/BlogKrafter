using Heroicsoft.NetForge.Components.Layout;

namespace BlogKrafter.Components;

/// <summary>
/// The layout of the editor screens: the host's <see cref="BlogKrafterOptions.AdminLayout"/> when it set
/// one, otherwise the NetForge shell. Either must mount the NetForge dialog host, which the shell does.
/// </summary>
public partial class BlogAdminLayout
{
    [Inject]
    private IOptions<BlogKrafterOptions> Options { get; set; } = default!;

    private Type Layout => Options.Value.AdminLayout ?? typeof(ShellLayout);
}