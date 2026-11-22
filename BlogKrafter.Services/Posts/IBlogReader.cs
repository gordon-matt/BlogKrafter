namespace BlogKrafter.Posts;

/// <summary>
/// What anonymous readers can see: only published posts whose date has arrived, in the current tenant.
/// The public pages, feeds and widgets read through this.
/// </summary>
public interface IBlogReader
{
    /// <summary>
    /// A page of visible posts, newest first. Search matches the title, the excerpt, and the source.
    /// An unknown tag or category slug is not found. The page size comes from the options.
    /// </summary>
    /// <param name="query">The page, tag, category, and search.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The page, with the tag or category it was filtered by, or not found.</returns>
    Task<Result<PostListResult>> ListPostsAsync(PostListQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// A post by its slug. With <paramref name="preview"/>, drafts and scheduled posts are returned too,
    /// for a signed-in caller only; the page is responsible for checking that caller may edit the blog.
    /// An old slug returns the post's current slug so the caller can redirect. A live post also includes the previous, next, and related posts.
    /// </summary>
    /// <param name="slug">The address segment. It is trimmed and compared in lower case.</param>
    /// <param name="preview"><see langword="true"/> includes drafts and scheduled posts. An anonymous caller is forbidden.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The post, a redirect slug, not found, or forbidden.</returns>
    Task<Result<PostLookup>> GetPostAsync(string slug, bool preview = false, CancellationToken cancellationToken = default);

    /// <summary>Visible posts grouped by the year of their publish date in the blog's time zone, newest year first.</summary>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The years. A year with no posts is omitted.</returns>
    Task<Result<IReadOnlyList<ArchiveYear>>> GetArchiveAsync(CancellationToken cancellationToken = default);

    /// <summary>Every category, with the number of visible posts in each.</summary>
    Task<Result<IReadOnlyList<CategoryDto>>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>Tags that have at least one visible post, most used first.</summary>
    Task<Result<IReadOnlyList<TagDto>>> GetTagsAsync(CancellationToken cancellationToken = default);

    /// <summary>The newest visible posts. A count of zero or less returns an empty list. An unknown tag or category slug is not found.</summary>
    /// <param name="count">How many posts to return.</param>
    /// <param name="tagSlug">When set, only posts with this tag.</param>
    /// <param name="categorySlug">When set, only posts in this category.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The posts, or not found.</returns>
    Task<Result<IReadOnlyList<PostSummaryDto>>> GetRecentPostsAsync(
        int count,
        string? tagSlug = null,
        string? categorySlug = null,
        CancellationToken cancellationToken = default);

    /// <summary>The newest visible posts, with their rendered bodies, up to the configured feed size.</summary>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The feed items.</returns>
    Task<Result<IReadOnlyList<FeedItem>>> GetFeedItemsAsync(CancellationToken cancellationToken = default);

    /// <summary>The blog index, every visible post, and the categories and tags that have at least one visible post.</summary>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The entries. Paths follow the configured base path.</returns>
    Task<Result<IReadOnlyList<SitemapEntry>>> GetSitemapEntriesAsync(CancellationToken cancellationToken = default);
}