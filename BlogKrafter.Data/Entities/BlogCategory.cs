namespace BlogKrafter.Data.Entities;

/// <summary>A category. Its slug is unique within a tenant. Deleting it leaves the posts and clears their category.</summary>
public class BlogCategory : BaseEntity<int>
{
    /// <summary>Empty for a single-site host. Not nullable, so the unique slug index behaves the same on every provider.</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>The name shown in the sidebar and on the category page.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The address segment. Unique with <see cref="TenantId"/>.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Optional text shown with the category. <see langword="null"/> when the editor left it blank.</summary>
    public string? Description { get; set; }

    /// <summary>Sort order in the editor and the sidebar. Lower comes first, then the name.</summary>
    public int SortOrder { get; set; }
}

/// <summary>Maps a category and its unique slug per tenant.</summary>
public sealed class BlogCategoryMap : IEntityTypeConfiguration<BlogCategory>
{
    /// <summary>Maps the categories table and the unique slug index.</summary>
    /// <param name="builder">The category builder.</param>
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