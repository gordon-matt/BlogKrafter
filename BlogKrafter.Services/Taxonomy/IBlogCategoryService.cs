namespace BlogKrafter.Taxonomy;

/// <summary>The editor's categories. <see cref="CategoryDto.PostCount"/> counts every post, drafts included.</summary>
public interface IBlogCategoryService
{
    /// <summary>
    /// Lists the value.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the outcome for the categories.</returns>
    Task<Result<IReadOnlyList<CategoryDto>>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the value.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the outcome for the category.</returns>
    Task<Result<CategoryDto>> SaveAsync(SaveCategoryRequest request, CancellationToken cancellationToken = default);

    /// <summary>Posts in the category are kept and become uncategorised.</summary>
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}