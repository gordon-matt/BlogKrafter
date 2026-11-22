namespace BlogKrafter.Data.Entities;

/// <summary>A tag. Tags are created when a post is saved with a name that does not exist yet.</summary>
public class BlogTag : BaseEntity<int>
{
    /// <summary>Empty for a single-site host. Not nullable, so the unique slug index behaves the same on every provider.</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>The name authors type and readers see.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The address segment, derived from <see cref="Name"/>. Unique with <see cref="TenantId"/>.</summary>
    public string Slug { get; set; } = string.Empty;
}

/// <summary>Maps a tag and its unique slug per tenant.</summary>
public sealed class BlogTagMap : IEntityTypeConfiguration<BlogTag>
{
    /// <summary>Maps the tags table and the unique slug index.</summary>
    /// <param name="builder">The tag builder.</param>
    public void Configure(EntityTypeBuilder<BlogTag> builder)
    {
        builder.ToTable(Constants.Tables.Tags);
        builder.HasKey(m => m.Id);

        builder.Property(m => m.TenantId).IsRequired().HasMaxLength(Constants.MaxLengths.TenantId).IsUnicode(false);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(Constants.MaxLengths.TagName).IsUnicode(true);
        builder.Property(m => m.Slug).IsRequired().HasMaxLength(Constants.MaxLengths.Slug).IsUnicode(false);

        builder.HasIndex(m => new { m.TenantId, m.Slug }).IsUnique();
    }
}