namespace BlogKrafter.Taxonomy;

/// <summary>The editor's tags. Tags are created by tagging a post, so there is no create call.</summary>
public interface IBlogTagService
{
    /// <summary>Every tag with the number of posts using it, drafts included.</summary>
    Task<Result<IReadOnlyList<TagDto>>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Names starting with or containing <paramref name="term"/>, for the tag picker. Names that start with the term come first.
    /// At most ten are returned, and their post counts are zero.
    /// </summary>
    /// <param name="term">The text the author has typed. Blank matches every name.</param>
    /// <param name="cancellationToken">Cancels the search.</param>
    /// <returns>The matching tags, or forbidden when nobody is signed in.</returns>
    Task<Result<IReadOnlyList<TagDto>>> SearchAsync(string? term, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renames a tag. When the new name belongs to another tag, the two are merged: this tag's posts
    /// move to the other one and this tag is removed. A name that produces an empty slug, or one longer than the tag column, is invalid.
    /// </summary>
    /// <param name="id">The tag to rename.</param>
    /// <param name="name">The new name. The slug is derived from it.</param>
    /// <param name="cancellationToken">Cancels the rename.</param>
    /// <returns>The tag that remains, or invalid, not found, or forbidden when nobody is signed in.</returns>
    Task<Result<TagDto>> RenameAsync(int id, string name, CancellationToken cancellationToken = default);

    /// <summary>Deletes a tag and its links to posts. The posts themselves stay.</summary>
    /// <param name="id">The tag id.</param>
    /// <param name="cancellationToken">Cancels the delete.</param>
    /// <returns>Success, not found, or forbidden when nobody is signed in.</returns>
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}