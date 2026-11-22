namespace BlogKrafter.Data.Entities;

/// <summary>A post. The rendered HTML is stored beside the source so a page view does not run Markdown again.</summary>
public class BlogPost : BaseEntity<int>
{
    /// <summary>Empty for a single-site host. Not nullable, so the unique slug index behaves the same on every provider.</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>The title shown on the post and in lists.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>The address segment. Unique with <see cref="TenantId"/>.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>What the author typed as the summary. Null means <see cref="Excerpt"/> is taken from the body.</summary>
    public string? Summary { get; set; }

    /// <summary>The summary shown on lists and in feeds, worked out on save.</summary>
    public string Excerpt { get; set; } = string.Empty;

    /// <summary>Whether <see cref="Content"/> is Markdown or HTML.</summary>
    public ContentFormat ContentFormat { get; set; }

    /// <summary>The source as written, in <see cref="ContentFormat"/>.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Rendered and sanitized on save, so a page view never runs the Markdown pipeline.</summary>
    public string ContentHtml { get; set; } = string.Empty;

    /// <summary>A site path or an http(s) address. <see langword="null"/> means no cover.</summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>The cover's alternative text. <see langword="null"/> when the author left it blank.</summary>
    public string? CoverImageAlt { get; set; }

    /// <summary>The title for search results. <see langword="null"/> uses <see cref="Title"/>.</summary>
    public string? MetaTitle { get; set; }

    /// <summary>The description for search results. <see langword="null"/> uses the excerpt.</summary>
    public string? MetaDescription { get; set; }

    /// <summary>A full http(s) address that should be the post's canonical link. <see langword="null"/> uses the blog address.</summary>
    public string? CanonicalUrl { get; set; }

    /// <summary>Draft or published. A published post with a future <see cref="PublishedAt"/> is scheduled.</summary>
    public PostStatus Status { get; set; }

    /// <summary>UTC. Set when a post is first published; a future value schedules it.</summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary><see langword="true"/> when the post is marked featured.</summary>
    public bool IsFeatured { get; set; }

    /// <summary><see langword="true"/> when the comments component may be shown. The default is <see langword="true"/>.</summary>
    public bool CommentsEnabled { get; set; } = true;

    /// <summary>Minutes to read, worked out on save. Zero when the body has no words.</summary>
    public int ReadingTimeMinutes { get; set; }

    /// <summary>The author's id, copied when the post is created.</summary>
    public string AuthorUserId { get; set; } = string.Empty;

    /// <summary>Copied from the author on create, so a post keeps its byline if the account goes away.</summary>
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>When the post was created, in UTC.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>When the post was last saved, in UTC.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>The category, or <see langword="null"/> when the post has none.</summary>
    public int? CategoryId { get; set; }

    /// <summary>The category. Deleting the category clears this.</summary>
    public BlogCategory? Category { get; set; }

    /// <summary>The tag links. Replacing a post's tags deletes these rows and inserts the new set.</summary>
    public ICollection<BlogPostTag> Tags { get; set; } = [];
}

/// <summary>Maps a post: required columns, UTC dates, a unique slug per tenant, and a category that is cleared when the category is deleted.</summary>
public sealed class BlogPostMap : IEntityTypeConfiguration<BlogPost>
{
    /// <summary>Maps the posts table, the column lengths, the UTC dates, and the unique slug index.</summary>
    /// <param name="builder">The post builder.</param>
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