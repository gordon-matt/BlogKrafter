namespace BlogKrafter.Posts;

/// <summary>
/// What anonymous readers can see: only published posts whose date has arrived, in the current tenant.
/// The public pages, feeds and widgets read through this.
/// </summary>
public interface IBlogReader
{
    /// <summary>NotFound when the tag or category slug does not exist.</summary>
    Task<Result<PostListResult>> ListPostsAsync(PostListQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// A post by its slug. With <paramref name="preview"/>, drafts and scheduled posts are returned too,
    /// for a signed-in caller only; the page is responsible for checking that caller may edit the blog.
    /// </summary>
    Task<Result<PostLookup>> GetPostAsync(string slug, bool preview = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the archive.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the outcome for the archive years.</returns>
    Task<Result<IReadOnlyList<ArchiveYear>>> GetArchiveAsync(CancellationToken cancellationToken = default);

    /// <summary>Every category, with the number of visible posts in each.</summary>
    Task<Result<IReadOnlyList<CategoryDto>>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>Tags that have at least one visible post, most used first.</summary>
    Task<Result<IReadOnlyList<TagDto>>> GetTagsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the recent posts.
    /// </summary>
    /// <param name="count">The count.</param>
    /// <param name="tagSlug">The tag slug.</param>
    /// <param name="categorySlug">The category slug.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the outcome for the post summaries.</returns>
    Task<Result<IReadOnlyList<PostSummaryDto>>> GetRecentPostsAsync(
        int count,
        string? tagSlug = null,
        string? categorySlug = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the feed items.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the outcome for the feed items.</returns>
    Task<Result<IReadOnlyList<FeedItem>>> GetFeedItemsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the sitemap entries.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the outcome for the sitemap entries.</returns>
    Task<Result<IReadOnlyList<SitemapEntry>>> GetSitemapEntriesAsync(CancellationToken cancellationToken = default);
}