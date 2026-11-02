namespace BlogKrafter.Posts;

/// <summary>Query fragments shared by the public reader and the editor, so "visible" means one thing everywhere.</summary>
internal static class PostQueries
{
    /// <summary>Published, and the publish date has arrived. A scheduled post becomes visible on its own.</summary>
    public static Expression<Func<BlogPost, bool>> Visible(string tenantId, DateTime now) =>
        post => post.TenantId == tenantId
            && post.Status == PostStatus.Published
            && post.PublishedAt != null
            && post.PublishedAt <= now;

    public static Expression<Func<BlogPostTag, bool>> VisibleLink(string tenantId, DateTime now) =>
        link => link.Post!.TenantId == tenantId
            && link.Post.Status == PostStatus.Published
            && link.Post.PublishedAt != null
            && link.Post.PublishedAt <= now;

    public static readonly Expression<Func<BlogPost, PostSummaryDto>> Summary = post => new PostSummaryDto(
        post.Id,
        post.Title,
        post.Slug,
        post.Excerpt,
        post.CoverImageUrl,
        post.CoverImageAlt,
        post.PublishedAt,
        post.UpdatedAt,
        post.ReadingTimeMinutes,
        post.AuthorName,
        post.IsFeatured,
        post.Category == null ? null : new CategoryRef(post.Category.Id, post.Category.Name, post.Category.Slug),
        post.Tags
            .OrderBy(link => link.Tag!.Name)
            .Select(link => new TagRef(link.Tag!.Id, link.Tag.Name, link.Tag.Slug))
            .ToList());

    public static PostState StateOf(PostStatus status, DateTime? publishedAt, DateTime now) =>
        status == PostStatus.Draft
            ? PostState.Draft
            : publishedAt is null || publishedAt > now ? PostState.Scheduled : PostState.Published;

    public static Expression<Func<IQueryable<BlogPost>, IQueryable<BlogPost>>> Newest =>
        query => query.OrderByDescending(post => post.PublishedAt).ThenByDescending(post => post.Id);
}
