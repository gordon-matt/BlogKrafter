namespace BlogKrafter.Posts;

/// <summary>
/// The editor's view of posts: every state, in the current tenant. Each call needs a signed-in user;
/// which users may edit is decided by the <c>BlogKrafter.Manage</c> policy on the screens.
/// </summary>
public interface IBlogPostService
{
    /// <summary>
    /// Lists the value.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the outcome for the admin post row paged list.</returns>
    Task<Result<PagedList<AdminPostRow>>> ListAsync(AdminPostQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the outcome for the post edit.</returns>
    Task<Result<PostEditDto>> GetAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates a post. Renaming a live post keeps its old address working as a redirect.
    /// Unknown tag names become new tags.
    /// </summary>
    Task<Result<PostEditDto>> SaveAsync(SavePostRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the value.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result.</returns>
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}