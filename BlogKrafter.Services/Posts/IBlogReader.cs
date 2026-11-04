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

    Task<Result<IReadOnlyList<ArchiveYear>>> GetArchiveAsync(CancellationToken cancellationToken = default);

    /// <summary>Every category, with the number of visible posts in each.</summary>
    Task<Result<IReadOnlyList<CategoryDto>>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>Tags that have at least one visible post, most used first.</summary>
    Task<Result<IReadOnlyList<TagDto>>> GetTagsAsync(CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<PostSummaryDto>>> GetRecentPostsAsync(
        int count,
        string? tagSlug = null,
        string? categorySlug = null,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<FeedItem>>> GetFeedItemsAsync(CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<SitemapEntry>>> GetSitemapEntriesAsync(CancellationToken cancellationToken = default);
}