namespace BlogKrafter.Posts;

/// <summary>
/// The editor's view of posts: every state, in the current tenant. Each call needs a signed-in user;
/// which users may edit is decided by the <c>BlogKrafter.Manage</c> policy on the screens.
/// </summary>
public interface IBlogPostService
{
    /// <summary>
    /// Lists the tenant's posts, newest edit first. Drafts, scheduled posts, and live posts are included.
    /// Search matches the title and the slug. The page size is clamped from 1 to 200.
    /// </summary>
    /// <param name="query">The page, filter, and search.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The page, or forbidden when nobody is signed in.</returns>
    Task<Result<PagedList<AdminPostRow>>> ListAsync(AdminPostQuery query, CancellationToken cancellationToken = default);

    /// <summary>Loads a post for the editor, including drafts. The body is the source, not the rendered HTML.</summary>
    /// <param name="id">The post id.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The post, not found, or forbidden when nobody is signed in.</returns>
    Task<Result<PostEditDto>> GetAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates a post. Renaming a live post keeps its old address working as a redirect.
    /// Unknown tag names become new tags. A blank slug is derived from the title, with a numeric suffix until it is free.
    /// The title is required. A published post also needs content. The HTML is rendered and sanitized before it is stored.
    /// </summary>
    /// <param name="request">The post. A null request is invalid.</param>
    /// <param name="cancellationToken">Cancels the save.</param>
    /// <returns>The saved post, or invalid, not found, forbidden, or an error when the tenant id is longer than 64 characters.</returns>
    Task<Result<PostEditDto>> SaveAsync(SavePostRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a post. Its tag links and old-address redirects are removed with it.</summary>
    /// <param name="id">The post id.</param>
    /// <param name="cancellationToken">Cancels the delete.</param>
    /// <returns>Success, not found, or forbidden when nobody is signed in.</returns>
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}