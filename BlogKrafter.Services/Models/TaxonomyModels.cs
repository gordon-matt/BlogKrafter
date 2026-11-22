namespace BlogKrafter.Models;

/// <summary><see cref="PostCount"/> counts posts readers can see on public lists, and every post on editor lists.</summary>
public sealed record CategoryDto(int Id, string Name, string Slug, string? Description, int SortOrder, int PostCount);

/// <summary>A tag and how many posts use it. Suggestions from the tag search report a count of zero.</summary>
/// <param name="Id">The tag id.</param>
/// <param name="Name">The name authors type.</param>
/// <param name="Slug">The address segment.</param>
/// <param name="PostCount">Posts using the tag, or zero on a search suggestion.</param>
public sealed record TagDto(int Id, string Name, string Slug, int PostCount);

/// <summary>A category to create or update. A missing id creates one.</summary>
public sealed record SaveCategoryRequest
{
    /// <summary>The category to update. <see langword="null"/> creates one.</summary>
    public int? Id { get; set; }

    /// <summary>The name. Required.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Empty to derive one from the name.</summary>
    public string? Slug { get; set; }

    /// <summary>Optional text shown with the category.</summary>
    public string? Description { get; set; }

    /// <summary>Lower comes first. The default is 0.</summary>
    public int SortOrder { get; set; }
}