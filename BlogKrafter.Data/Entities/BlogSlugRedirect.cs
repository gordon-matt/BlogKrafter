namespace BlogKrafter.Data.Entities;

/// <summary>
/// A slug a published post used to have. Links and search results keep pointing at the old address,
/// so it is redirected to the post's current one rather than returning 404.
/// </summary>
public class BlogSlugRedirect : BaseEntity<int>
{
    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the slug.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the post identifier.
    /// </summary>
    public int PostId { get; set; }

    /// <summary>
    /// Gets or sets the created at.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Represents the blog slug redirect map.
/// </summary>
public sealed class BlogSlugRedirectMap : IEntityTypeConfiguration<BlogSlugRedirect>
{
    /// <summary>
    /// Configures the value.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<BlogSlugRedirect> builder)
    {
        builder.ToTable(Constants.Tables.SlugRedirects);
        builder.HasKey(m => m.Id);

        builder.Property(m => m.TenantId).IsRequired().HasMaxLength(Constants.MaxLengths.TenantId).IsUnicode(false);
        builder.Property(m => m.Slug).IsRequired().HasMaxLength(Constants.MaxLengths.Slug).IsUnicode(false);
        builder.Property(m => m.CreatedAt).HasConversion(UtcDateTimeConverter.Instance);

        builder.HasOne<BlogPost>()
            .WithMany()
            .HasForeignKey(m => m.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => new { m.TenantId, m.Slug }).IsUnique();
    }
}