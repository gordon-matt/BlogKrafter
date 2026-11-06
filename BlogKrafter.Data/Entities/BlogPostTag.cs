namespace BlogKrafter.Data.Entities;

/// <summary>
/// An explicit join row rather than an EF skip navigation, so a post's tags can be replaced with
/// one set-based delete and insert through the repository instead of tracking a loaded graph.
/// </summary>
public class BlogPostTag : IEntity
{
    /// <summary>
    /// Gets or sets the post identifier.
    /// </summary>
    public int PostId { get; set; }

    /// <summary>
    /// Gets or sets the tag identifier.
    /// </summary>
    public int TagId { get; set; }

    /// <summary>
    /// Gets or sets the post.
    /// </summary>
    public BlogPost? Post { get; set; }

    /// <summary>
    /// Gets or sets the tag.
    /// </summary>
    public BlogTag? Tag { get; set; }

    /// <summary>
    /// Gets the key values.
    /// </summary>
    public object[] KeyValues => [PostId, TagId];
}

/// <summary>
/// Represents the blog post tag map.
/// </summary>
public sealed class BlogPostTagMap : IEntityTypeConfiguration<BlogPostTag>
{
    /// <summary>
    /// Configures the value.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<BlogPostTag> builder)
    {
        builder.ToTable(Constants.Tables.PostTags);
        builder.HasKey(m => new { m.PostId, m.TagId });
        builder.Ignore(m => m.KeyValues);

        builder.HasOne(m => m.Post)
            .WithMany(m => m.Tags)
            .HasForeignKey(m => m.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Tag)
            .WithMany()
            .HasForeignKey(m => m.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.TagId);
    }
}