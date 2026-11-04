namespace BlogKrafter.Models;

/// <summary>What a reader sees of a post's state. Scheduled is a published post whose date has not arrived.</summary>
public enum PostState
{
    Draft,
    Scheduled,
    Published,
}

/// <summary>Filters the editor's post list.</summary>
public enum PostFilter
{
    All,
    Drafts,
    Scheduled,
    Published,
}

public sealed record CategoryRef(int Id, string Name, string Slug);

public sealed record TagRef(int Id, string Name, string Slug);

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

public sealed record PostListQuery
{
    public int Page { get; init; } = 1;

    public string? TagSlug { get; init; }

    public string? CategorySlug { get; init; }

    public string? Search { get; init; }
}

/// <summary>A page of posts, with the tag or category it was filtered by, so the page can title itself.</summary>
public sealed record PostListResult(PagedList<PostSummaryDto> Posts, TagDto? Tag, CategoryDto? Category);

/// <summary>A line in the archive. <see cref="PublishedAt"/> is already in the blog's time zone.</summary>
public sealed record ArchiveEntry(string Title, string Slug, DateTime PublishedAt);

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

public sealed record AdminPostQuery
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public PostFilter Filter { get; init; } = PostFilter.All;

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
    public int? Id { get; set; }

    public string Title { get; set; } = string.Empty;

    /// <summary>Empty to derive one from the title.</summary>
    public string? Slug { get; set; }

    public string? Summary { get; set; }

    public ContentFormat ContentFormat { get; set; }

    public string Content { get; set; } = string.Empty;

    public string? CoverImageUrl { get; set; }

    public string? CoverImageAlt { get; set; }

    public string? MetaTitle { get; set; }

    public string? MetaDescription { get; set; }

    public string? CanonicalUrl { get; set; }

    public PostStatus Status { get; set; }

    /// <summary>UTC. Left empty on a published post, it becomes the moment of saving.</summary>
    public DateTime? PublishedAt { get; set; }

    public bool IsFeatured { get; set; }

    public bool CommentsEnabled { get; set; } = true;

    public int? CategoryId { get; set; }

    /// <summary>Tag names. Unknown names become new tags.</summary>
    public IList<string> Tags { get; set; } = [];
}

public sealed record SitemapEntry(string Path, DateTime? LastModified);