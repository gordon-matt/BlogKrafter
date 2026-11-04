namespace BlogKrafter.Data.Entities;

public class BlogPost : BaseEntity<int>
{
    /// <summary>Empty for a single-site host. Not nullable, so the unique slug index behaves the same on every provider.</summary>
    public string TenantId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    /// <summary>What the author typed as the summary. Null means <see cref="Excerpt"/> is taken from the body.</summary>
    public string? Summary { get; set; }

    /// <summary>The summary shown on lists and in feeds, worked out on save.</summary>
    public string Excerpt { get; set; } = string.Empty;

    public ContentFormat ContentFormat { get; set; }

    /// <summary>The source as written, in <see cref="ContentFormat"/>.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Rendered and sanitized on save, so a page view never runs the Markdown pipeline.</summary>
    public string ContentHtml { get; set; } = string.Empty;

    public string? CoverImageUrl { get; set; }

    public string? CoverImageAlt { get; set; }

    public string? MetaTitle { get; set; }

    public string? MetaDescription { get; set; }

    public string? CanonicalUrl { get; set; }

    public PostStatus Status { get; set; }

    /// <summary>UTC. Set when a post is first published; a future value schedules it.</summary>
    public DateTime? PublishedAt { get; set; }

    public bool IsFeatured { get; set; }

    public bool CommentsEnabled { get; set; } = true;

    public int ReadingTimeMinutes { get; set; }

    public string AuthorUserId { get; set; } = string.Empty;

    /// <summary>Copied from the author on create, so a post keeps its byline if the account goes away.</summary>
    public string AuthorName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int? CategoryId { get; set; }

    public BlogCategory? Category { get; set; }

    public ICollection<BlogPostTag> Tags { get; set; } = [];
}

public sealed class BlogPostMap : IEntityTypeConfiguration<BlogPost>
{
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