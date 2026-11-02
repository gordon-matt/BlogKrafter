namespace BlogKrafter.Taxonomy;

/// <summary>The editor's tags. Tags are created by tagging a post, so there is no create call.</summary>
public interface IBlogTagService
{
    /// <summary>Every tag with the number of posts using it, drafts included.</summary>
    Task<Result<IReadOnlyList<TagDto>>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>Names starting with or containing <paramref name="term"/>, for the tag picker.</summary>
    Task<Result<IReadOnlyList<TagDto>>> SearchAsync(string? term, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renames a tag. When the new name belongs to another tag, the two are merged: this tag's posts
    /// move to the other one and this tag is removed.
    /// </summary>
    Task<Result<TagDto>> RenameAsync(int id, string name, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
