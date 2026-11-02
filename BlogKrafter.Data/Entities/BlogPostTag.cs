namespace BlogKrafter.Data.Entities;

/// <summary>
/// An explicit join row rather than an EF skip navigation, so a post's tags can be replaced with
/// one set-based delete and insert through the repository instead of tracking a loaded graph.
/// </summary>
public class BlogPostTag : IEntity
{
    public int PostId { get; set; }

    public int TagId { get; set; }

    public BlogPost? Post { get; set; }

    public BlogTag? Tag { get; set; }

    public object[] KeyValues => [PostId, TagId];
}

public sealed class BlogPostTagMap : IEntityTypeConfiguration<BlogPostTag>
{
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
