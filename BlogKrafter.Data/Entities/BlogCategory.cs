namespace BlogKrafter.Data.Entities;

/// <summary>
/// Represents the blog category.
/// </summary>
public class BlogCategory : BaseEntity<int>
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

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the sort order.
    /// </summary>
    public int SortOrder { get; set; }
}

/// <summary>
/// Represents the blog category map.
/// </summary>
public sealed class BlogCategoryMap : IEntityTypeConfiguration<BlogCategory>
{
    /// <summary>
    /// Configures the value.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<BlogCategory> builder)
    {
        builder.ToTable(Constants.Tables.Categories);
        builder.HasKey(m => m.Id);

        builder.Property(m => m.TenantId).IsRequired().HasMaxLength(Constants.MaxLengths.TenantId).IsUnicode(false);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(Constants.MaxLengths.CategoryName).IsUnicode(true);
        builder.Property(m => m.Slug).IsRequired().HasMaxLength(Constants.MaxLengths.Slug).IsUnicode(false);
        builder.Property(m => m.Description).HasMaxLength(Constants.MaxLengths.CategoryDescription).IsUnicode(true);

        builder.HasIndex(m => new { m.TenantId, m.Slug }).IsUnique();
    }
}