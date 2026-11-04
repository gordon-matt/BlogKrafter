namespace BlogKrafter.Posts;

/// <summary>
/// The editor's view of posts: every state, in the current tenant. Each call needs a signed-in user;
/// which users may edit is decided by the <c>BlogKrafter.Manage</c> policy on the screens.
/// </summary>
public interface IBlogPostService
{
    Task<Result<PagedList<AdminPostRow>>> ListAsync(AdminPostQuery query, CancellationToken cancellationToken = default);

    Task<Result<PostEditDto>> GetAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates a post. Renaming a live post keeps its old address working as a redirect.
    /// Unknown tag names become new tags.
    /// </summary>
    Task<Result<PostEditDto>> SaveAsync(SavePostRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}