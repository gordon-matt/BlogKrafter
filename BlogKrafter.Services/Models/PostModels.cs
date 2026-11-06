namespace BlogKrafter.Models;

/// <summary>What a reader sees of a post's state. Scheduled is a published post whose date has not arrived.</summary>
public enum PostState
{
    /// <summary>
    /// The draft value.
    /// </summary>
    Draft,
    /// <summary>
    /// The scheduled value.
    /// </summary>
    Scheduled,
    /// <summary>
    /// The published value.
    /// </summary>
    Published,
}

/// <summary>Filters the editor's post list.</summary>
public enum PostFilter
{
    /// <summary>
    /// The all value.
    /// </summary>
    All,
    /// <summary>
    /// The drafts value.
    /// </summary>
    Drafts,
    /// <summary>
    /// The scheduled value.
    /// </summary>
    Scheduled,
    /// <summary>
    /// The published value.
    /// </summary>
    Published,
}

/// <summary>
/// Represents the category ref.
/// </summary>
/// <param name="Id">The identifier.</param>
/// <param name="Name">The name.</param>
/// <param name="Slug">The slug.</param>
public sealed record CategoryRef(int Id, string Name, string Slug);

/// <summary>
/// Represents the tag ref.
/// </summary>
/// <param name="Id">The identifier.</param>
/// <param name="Name">The name.</param>
/// <param name="Slug">The slug.</param>
public sealed record TagRef(int Id, string Name, string Slug);

/// <summary>
/// Represents the post link.
/// </summary>
/// <param name="Title">The title.</param>
/// <param name="Slug">The slug.</param>
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

/// <summary>
/// Represents the post list query.
/// </summary>
public sealed record PostListQuery
{
    /// <summary>
    /// Gets or sets the page.
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Gets or sets the tag slug.
    /// </summary>
    public string? TagSlug { get; init; }

    /// <summary>
    /// Gets or sets the category slug.
    /// </summary>
    public string? CategorySlug { get; init; }

    /// <summary>
    /// Gets or sets the search.
    /// </summary>
    public string? Search { get; init; }
}

/// <summary>A page of posts, with the tag or category it was filtered by, so the page can title itself.</summary>
public sealed record PostListResult(PagedList<PostSummaryDto> Posts, TagDto? Tag, CategoryDto? Category);

/// <summary>A line in the archive. <see cref="PublishedAt"/> is already in the blog's time zone.</summary>
public sealed record ArchiveEntry(string Title, string Slug, DateTime PublishedAt);

/// <summary>
/// Represents the archive year.
/// </summary>
/// <param name="Year">The year.</param>
/// <param name="Posts">The posts.</param>
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

/// <summary>
/// Represents the admin post query.
/// </summary>
public sealed record AdminPostQuery
{
    /// <summary>
    /// Gets or sets the page.
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Gets or sets the filter.
    /// </summary>
    public PostFilter Filter { get; init; } = PostFilter.All;

    /// <summary>
    /// Gets or sets the search.
    /// </summary>
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
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Empty to derive one from the title.</summary>
    public string? Slug { get; set; }

    /// <summary>
    /// Gets or sets the summary.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Gets or sets the content format.
    /// </summary>
    public ContentFormat ContentFormat { get; set; }

    /// <summary>
    /// Gets or sets the content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cover image URL.
    /// </summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the cover image alt.
    /// </summary>
    public string? CoverImageAlt { get; set; }

    /// <summary>
    /// Gets or sets the meta title.
    /// </summary>
    public string? MetaTitle { get; set; }

    /// <summary>
    /// Gets or sets the meta description.
    /// </summary>
    public string? MetaDescription { get; set; }

    /// <summary>
    /// Gets or sets the canonical URL.
    /// </summary>
    public string? CanonicalUrl { get; set; }

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public PostStatus Status { get; set; }

    /// <summary>UTC. Left empty on a published post, it becomes the moment of saving.</summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is featured.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is comments enabled.
    /// </summary>
    public bool CommentsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the category identifier.
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>Tag names. Unknown names become new tags.</summary>
    public IList<string> Tags { get; set; } = [];
}

/// <summary>
/// Represents the sitemap entry.
/// </summary>
/// <param name="Path">The path.</param>
/// <param name="LastModified">The last modified.</param>
public sealed record SitemapEntry(string Path, DateTime? LastModified);