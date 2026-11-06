namespace BlogKrafter.Data.Entities;

/// <summary>
/// Represents the blog post.
/// </summary>
public class BlogPost : BaseEntity<int>
{
    /// <summary>Empty for a single-site host. Not nullable, so the unique slug index behaves the same on every provider.</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the slug.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>What the author typed as the summary. Null means <see cref="Excerpt"/> is taken from the body.</summary>
    public string? Summary { get; set; }

    /// <summary>The summary shown on lists and in feeds, worked out on save.</summary>
    public string Excerpt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content format.
    /// </summary>
    public ContentFormat ContentFormat { get; set; }

    /// <summary>The source as written, in <see cref="ContentFormat"/>.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Rendered and sanitized on save, so a page view never runs the Markdown pipeline.</summary>
    public string ContentHtml { get; set; } = string.Empty;

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

    /// <summary>UTC. Set when a post is first published; a future value schedules it.</summary>
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
    /// Gets or sets the reading time minutes.
    /// </summary>
    public int ReadingTimeMinutes { get; set; }

    /// <summary>
    /// Gets or sets the author user identifier.
    /// </summary>
    public string AuthorUserId { get; set; } = string.Empty;

    /// <summary>Copied from the author on create, so a post keeps its byline if the account goes away.</summary>
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the created at.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the updated at.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the category identifier.
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the category.
    /// </summary>
    public BlogCategory? Category { get; set; }

    /// <summary>
    /// Gets or sets the tags.
    /// </summary>
    public ICollection<BlogPostTag> Tags { get; set; } = [];
}

/// <summary>
/// Represents the blog post map.
/// </summary>
public sealed class BlogPostMap : IEntityTypeConfiguration<BlogPost>
{
    /// <summary>
    /// Configures the value.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.ToTable(Constants.Tables.Posts);
        builder.HasKey(m => m.Id);

        builder.Property(m => m.TenantId).IsRequired().HasMaxLength(Constants.MaxLengths.TenantId).IsUnicode(false);
        builder.Property(m => m.Title).IsRequired().HasMaxLength(Constants.MaxLengths.Title).IsUnicode(true);
        builder.Property(m => m.Slug).IsRequired().HasMaxLength(Constants.MaxLengths.Slug).IsUnicode(false);
        builder.Property(m => m.Summary).HasMaxLength(Constants.MaxLengths.Summary).IsUnicode(true);
        builder.Property(m => m.Excerpt).IsRequired().HasMaxLength(Constants.MaxLengths.Summary).IsUnicode(true);
        builder.Property(m => m.Content).IsRequired().IsUnicode(true);
        builder.Property(m => m.ContentHtml).IsRequired().IsUnicode(true);
        builder.Property(m => m.CoverImageUrl).HasMaxLength(Constants.MaxLengths.Url).IsUnicode(true);
        builder.Property(m => m.CoverImageAlt).HasMaxLength(Constants.MaxLengths.ImageAlt).IsUnicode(true);
        builder.Property(m => m.MetaTitle).HasMaxLength(Constants.MaxLengths.Title).IsUnicode(true);
        builder.Property(m => m.MetaDescription).HasMaxLength(Constants.MaxLengths.MetaDescription).IsUnicode(true);
        builder.Property(m => m.CanonicalUrl).HasMaxLength(Constants.MaxLengths.Url).IsUnicode(true);
        builder.Property(m => m.AuthorUserId).IsRequired().HasMaxLength(Constants.MaxLengths.UserId).IsUnicode(true);
        builder.Property(m => m.AuthorName).IsRequired().HasMaxLength(Constants.MaxLengths.AuthorName).IsUnicode(true);
        builder.Property(m => m.PublishedAt).HasConversion(UtcNullableDateTimeConverter.Instance);
        builder.Property(m => m.CreatedAt).HasConversion(UtcDateTimeConverter.Instance);
        builder.Property(m => m.UpdatedAt).HasConversion(UtcDateTimeConverter.Instance);

        builder.HasOne(m => m.Category)
            .WithMany()
            .HasForeignKey(m => m.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(m => new { m.TenantId, m.Slug }).IsUnique();
        builder.HasIndex(m => new { m.TenantId, m.Status, m.PublishedAt });
    }
}