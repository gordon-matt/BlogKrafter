namespace BlogKrafter.Models;

/// <summary><see cref="PostCount"/> counts posts readers can see on public lists, and every post on editor lists.</summary>
public sealed record CategoryDto(int Id, string Name, string Slug, string? Description, int SortOrder, int PostCount);

/// <summary>
/// Represents the tag DTO.
/// </summary>
/// <param name="Id">The identifier.</param>
/// <param name="Name">The name.</param>
/// <param name="Slug">The slug.</param>
/// <param name="PostCount">The post count.</param>
public sealed record TagDto(int Id, string Name, string Slug, int PostCount);

/// <summary>
/// Represents the save category request.
/// </summary>
public sealed record SaveCategoryRequest
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Empty to derive one from the name.</summary>
    public string? Slug { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the sort order.
    /// </summary>
    public int SortOrder { get; set; }
}