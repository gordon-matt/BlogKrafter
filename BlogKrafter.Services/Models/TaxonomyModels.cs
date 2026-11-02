namespace BlogKrafter.Models;

/// <summary><see cref="PostCount"/> counts posts readers can see on public lists, and every post on editor lists.</summary>
public sealed record CategoryDto(int Id, string Name, string Slug, string? Description, int SortOrder, int PostCount);

public sealed record TagDto(int Id, string Name, string Slug, int PostCount);

public sealed record SaveCategoryRequest
{
    public int? Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Empty to derive one from the name.</summary>
    public string? Slug { get; set; }

    public string? Description { get; set; }

    public int SortOrder { get; set; }
}
