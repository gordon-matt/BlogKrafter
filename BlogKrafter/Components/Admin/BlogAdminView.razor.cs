using System.Globalization;

namespace BlogKrafter.Components.Admin;

/// <summary>
/// The editor screens, chosen from the route values. Render it from a page guarded by the
/// <c>BlogKrafter.Manage</c> policy whose routes match <see cref="BlogKrafterOptions.AdminPath"/> to move
/// the editor: <c>{admin}</c>, <c>{admin}/{Section}</c> and <c>{admin}/{Section}/{Id}</c>.
/// </summary>
public partial class BlogAdminView
{
    /// <summary><c>posts</c>, <c>categories</c> or <c>tags</c>. Empty means posts.</summary>
    [Parameter]
    public string? Section { get; set; }

    /// <summary>A post id, or <c>new</c>.</summary>
    [Parameter]
    public string? Id { get; set; }

    private AdminSection? CurrentSection => (Section ?? string.Empty).ToLowerInvariant() switch
    {
        "" or "posts" => AdminSection.Posts,
        "categories" => AdminSection.Categories,
        "tags" => AdminSection.Tags,
        _ => null,
    };

    /// <summary>Null for a new post. An id that is not a number is passed on as 0, which no post has.</summary>
    private int? PostId => string.Equals(Id, "new", StringComparison.OrdinalIgnoreCase)
        ? null
        : int.TryParse(Id, NumberStyles.None, CultureInfo.InvariantCulture, out int id) ? id : 0;
}

/// <summary>The editor screens <see cref="BlogAdminView"/> can show.</summary>
public enum AdminSection
{
    /// <summary>
    /// The posts value.
    /// </summary>
    Posts,
    /// <summary>
    /// The categories value.
    /// </summary>
    Categories,
    /// <summary>
    /// The tags value.
    /// </summary>
    Tags,
}