namespace BlogKrafter.Taxonomy;

/// <summary>The editor's categories. <see cref="CategoryDto.PostCount"/> counts every post, drafts included.</summary>
public interface IBlogCategoryService
{
    /// <summary>Every category, ordered by sort order and then name. The post count includes drafts.</summary>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The categories, or forbidden when nobody is signed in.</returns>
    Task<Result<IReadOnlyList<CategoryDto>>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates a category. A blank slug is derived from the name. The name is required, and the slug must be unique in the tenant.
    /// </summary>
    /// <param name="request">The category.</param>
    /// <param name="cancellationToken">Cancels the save.</param>
    /// <returns>The category, or invalid, not found, or forbidden when nobody is signed in.</returns>
    Task<Result<CategoryDto>> SaveAsync(SaveCategoryRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a category. Posts in it are kept and become uncategorised.</summary>
    /// <param name="id">The category id.</param>
    /// <param name="cancellationToken">Cancels the delete.</param>
    /// <returns>Success, not found, or forbidden when nobody is signed in.</returns>
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}