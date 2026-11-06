namespace BlogKrafter.Data.Entities;

/// <summary>
/// Represents the blog tag.
/// </summary>
public class BlogTag : BaseEntity<int>
{
    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the slug.
    /// </summary>
    public string Slug { get; set; } = string.Empty;
}

/// <summary>
/// Represents the blog tag map.
/// </summary>
public sealed class BlogTagMap : IEntityTypeConfiguration<BlogTag>
{
    /// <summary>
    /// Configures the value.
    /// </summary>
    /// <param name="builder">The builder.</param>
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