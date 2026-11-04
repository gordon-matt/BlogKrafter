namespace BlogKrafter.Taxonomy;

/// <summary>The editor's categories. <see cref="CategoryDto.PostCount"/> counts every post, drafts included.</summary>
public interface IBlogCategoryService
{
    Task<Result<IReadOnlyList<CategoryDto>>> ListAsync(CancellationToken cancellationToken = default);

    Task<Result<CategoryDto>> SaveAsync(SaveCategoryRequest request, CancellationToken cancellationToken = default);

    /// <summary>Posts in the category are kept and become uncategorised.</summary>
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}