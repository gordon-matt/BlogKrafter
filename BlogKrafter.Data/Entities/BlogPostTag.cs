namespace BlogKrafter.Data.Entities;

/// <summary>
/// An explicit join row rather than an EF skip navigation, so a post's tags can be replaced with
/// one set-based delete and insert through the repository instead of tracking a loaded graph.
/// </summary>
public class BlogPostTag : IEntity
{
    /// <summary>The post. Deleting the post deletes this row.</summary>
    public int PostId { get; set; }

    /// <summary>The tag. Deleting the tag deletes this row.</summary>
    public int TagId { get; set; }

    /// <summary>The post this row links.</summary>
    public BlogPost? Post { get; set; }

    /// <summary>The tag this row links.</summary>
    public BlogTag? Tag { get; set; }

    /// <summary>The composite key, <see cref="PostId"/> then <see cref="TagId"/>, for the repository.</summary>
    public object[] KeyValues => [PostId, TagId];
}

/// <summary>Maps the join table. Both foreign keys cascade, so deleting a post or a tag removes the links.</summary>
public sealed class BlogPostTagMap : IEntityTypeConfiguration<BlogPostTag>
{
    /// <summary>Maps the composite key and the cascading relationships.</summary>
    /// <param name="builder">The join-row builder.</param>
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