namespace BlogKrafter.Data.Entities;

/// <summary>
/// A slug a published post used to have. Links and search results keep pointing at the old address,
/// so it is redirected to the post's current one rather than returning 404.
/// </summary>
public class BlogSlugRedirect : BaseEntity<int>
{
    /// <summary>Empty for a single-site host. The old slug is unique with this value.</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>The address the post used to have.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>The post the old address now points at. Deleting the post deletes the redirect.</summary>
    public int PostId { get; set; }

    /// <summary>When the redirect was recorded, in UTC.</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>Maps an old slug. The pair of tenant and slug is unique, and deleting the post deletes the row.</summary>
public sealed class BlogSlugRedirectMap : IEntityTypeConfiguration<BlogSlugRedirect>
{
    /// <summary>Maps the redirects table, the UTC timestamp, and the unique slug index.</summary>
    /// <param name="builder">The redirect builder.</param>
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