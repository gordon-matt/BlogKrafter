namespace BlogKrafter.Models;

/// <summary>What a reader sees of a post's state. Scheduled is a published post whose date has not arrived.</summary>
public enum PostState
{
    /// <summary>Not visible. It has not been published.</summary>
    Draft,

    /// <summary>Published, but the publish date has not arrived, so readers cannot see it yet.</summary>
    Scheduled,

    /// <summary>Visible. The publish date is empty or has arrived.</summary>
    Published,
}

/// <summary>Filters the editor's post list.</summary>
public enum PostFilter
{
    /// <summary>Drafts, scheduled posts, and live posts.</summary>
    All,

    /// <summary>Posts that have not been published.</summary>
    Drafts,

    /// <summary>Published posts whose date has not arrived.</summary>
    Scheduled,

    /// <summary>Posts readers can see.</summary>
    Published,
}

/// <summary>A category named on a post, enough to link to it.</summary>
/// <param name="Id">The category id.</param>
/// <param name="Name">The name shown beside the post.</param>
/// <param name="Slug">The address segment.</param>
public sealed record CategoryRef(int Id, string Name, string Slug);

/// <summary>A tag named on a post, enough to link to it.</summary>
/// <param name="Id">The tag id.</param>
/// <param name="Name">The name shown on the post.</param>
/// <param name="Slug">The address segment.</param>
public sealed record TagRef(int Id, string Name, string Slug);

/// <summary>A neighbouring post, enough for a previous or next link.</summary>
/// <param name="Title">The title of that post.</param>
/// <param name="Slug">The address segment of that post.</param>
public sealed record PostLink(string Title, string Slug);

/// <summary>A heading the table of contents links to. <see cref="Id"/> is the anchor written into the post.</summary>
public sealed record TocEntry(string Id, string Text, int Level);

/// <summary>A post as it appears on a list, a feed or a related-posts strip: no body.</summary>
public sealed record PostSummaryDto(
    int Id,
    string Title,
    string Slug,
    string Excerpt,
    string? CoverImageUrl,
    string? CoverImageAlt,
    DateTime? PublishedAt,
    DateTime UpdatedAt,
    int ReadingTimeMinutes,
    string AuthorName,
    bool IsFeatured,
    CategoryRef? Category,
    IReadOnlyList<TagRef> Tags);

/// <summary>
/// A post with its rendered body, as the public post page shows it. <see cref="Summary"/> is only what
/// the author wrote; <see cref="Excerpt"/> falls back to the opening of the body, which the page
/// already shows, so it suits descriptions and cards but not a lead paragraph.
/// </summary>
public sealed record PostDto(
    int Id,
    string Title,
    string Slug,
    string Excerpt,
    string? Summary,
    string ContentHtml,
    string? CoverImageUrl,
    string? CoverImageAlt,
    string? MetaTitle,
    string? MetaDescription,
    string? CanonicalUrl,
    PostState State,
    DateTime? PublishedAt,
    DateTime UpdatedAt,
    int ReadingTimeMinutes,
    string AuthorName,
    bool CommentsEnabled,
    CategoryRef? Category,
    IReadOnlyList<TagRef> Tags,
    IReadOnlyList<TocEntry> TableOfContents,
    PostLink? Previous,
    PostLink? Next,
    IReadOnlyList<PostSummaryDto> Related);

/// <summary>
/// The answer to a slug. <see cref="RedirectSlug"/> is set instead of <see cref="Post"/> when the slug
/// belonged to a post that has since been renamed.
/// </summary>
public sealed record PostLookup(PostDto? Post, string? RedirectSlug);

/// <summary>Which public page to load. A page below 1 is read as 1.</summary>
public sealed record PostListQuery
{
    /// <summary>The page, starting at 1.</summary>
    public int Page { get; init; } = 1;

    /// <summary>When set, only posts with this tag. An unknown slug is not found.</summary>
    public string? TagSlug { get; init; }

    /// <summary>When set, only posts in this category. An unknown slug is not found.</summary>
    public string? CategorySlug { get; init; }

    /// <summary>When set, posts whose title, excerpt, or source contains this text.</summary>
    public string? Search { get; init; }
}

/// <summary>A page of posts, with the tag or category it was filtered by, so the page can title itself.</summary>
public sealed record PostListResult(PagedList<PostSummaryDto> Posts, TagDto? Tag, CategoryDto? Category);

/// <summary>A line in the archive. <see cref="PublishedAt"/> is already in the blog's time zone.</summary>
public sealed record ArchiveEntry(string Title, string Slug, DateTime PublishedAt);

/// <summary>One year of the archive, newest post first.</summary>
/// <param name="Year">The year in the blog's time zone.</param>
/// <param name="Posts">The visible posts published in that year.</param>
public sealed record ArchiveYear(int Year, IReadOnlyList<ArchiveEntry> Posts);

/// <summary>A post as the editor's list shows it.</summary>
public sealed record AdminPostRow(
    int Id,
    string Title,
    string Slug,
    PostState State,
    DateTime? PublishedAt,
    DateTime UpdatedAt,
    string AuthorName,
    string? CategoryName,
    bool IsFeatured);

/// <summary>Which editor page to load. The page size is clamped from 1 to 200.</summary>
public sealed record AdminPostQuery
{
    /// <summary>The page, starting at 1. A lower value is read as 1.</summary>
    public int Page { get; init; } = 1;

    /// <summary>Rows on the page. The default is 20, and the service clamps it from 1 to 200.</summary>
    public int PageSize { get; init; } = 20;

    /// <summary>Which states to include. The default is every state.</summary>
    public PostFilter Filter { get; init; } = PostFilter.All;

    /// <summary>When set, posts whose title or slug contains this text.</summary>
    public string? Search { get; init; }
}

/// <summary>A post as the editor loads it: the source, not the rendered body.</summary>
public sealed record PostEditDto(
    int Id,
    string Title,
    string Slug,
    string? Summary,
    ContentFormat ContentFormat,
    string Content,
    string? CoverImageUrl,
    string? CoverImageAlt,
    string? MetaTitle,
    string? MetaDescription,
    string? CanonicalUrl,
    PostStatus Status,
    PostState State,
    DateTime? PublishedAt,
    bool IsFeatured,
    bool CommentsEnabled,
    int? CategoryId,
    IReadOnlyList<string> Tags,
    string AuthorName,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>
/// Creates a post when <see cref="Id"/> is null, otherwise updates it. Settable so an edit form can bind
/// to it directly.
/// </summary>
public sealed record SavePostRequest
{
    /// <summary>The post to update. <see langword="null"/> creates one.</summary>
    public int? Id { get; set; }

    /// <summary>The title. Required.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Empty to derive one from the title.</summary>
    public string? Slug { get; set; }

    /// <summary>The author's summary. Blank falls back to an excerpt of the body.</summary>
    public string? Summary { get; set; }

    /// <summary>Whether <see cref="Content"/> is Markdown or HTML.</summary>
    public ContentFormat ContentFormat { get; set; }

    /// <summary>The source. Required when <see cref="Status"/> is published.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>A site path starting with one slash, or an absolute http(s) address. Blank means no cover.</summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>The cover's alternative text.</summary>
    public string? CoverImageAlt { get; set; }

    /// <summary>The title for search results. Blank uses the post title.</summary>
    public string? MetaTitle { get; set; }

    /// <summary>The description for search results. Blank uses the excerpt.</summary>
    public string? MetaDescription { get; set; }

    /// <summary>A full http(s) address for the canonical link. Blank uses the blog address.</summary>
    public string? CanonicalUrl { get; set; }

    /// <summary>Draft or published. Publishing with an empty date uses now, or keeps the date the post already has.</summary>
    public PostStatus Status { get; set; }

    /// <summary>UTC. Left empty on a published post, it becomes the moment of saving.</summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary><see langword="true"/> marks the post featured.</summary>
    public bool IsFeatured { get; set; }

    /// <summary><see langword="true"/> lets the comments component be shown. The default is <see langword="true"/>.</summary>
    public bool CommentsEnabled { get; set; } = true;

    /// <summary>The category, or <see langword="null"/> to leave the post uncategorised. An unknown id is invalid.</summary>
    public int? CategoryId { get; set; }

    /// <summary>Tag names. Unknown names become new tags.</summary>
    public IList<string> Tags { get; set; } = [];
}

/// <summary>One location in the sitemap. The path is root-relative and follows the configured base path.</summary>
/// <param name="Path">The path, such as <c>/blog/welcome</c>.</param>
/// <param name="LastModified">When the location last changed, in UTC, or <see langword="null"/> for the index when the blog has no posts.</param>
public sealed record SitemapEntry(string Path, DateTime? LastModified);