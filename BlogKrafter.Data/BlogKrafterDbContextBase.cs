namespace BlogKrafter.Data;

/// <summary>
/// Provider-agnostic context. Each <c>BlogKrafter.Data.{Provider}</c> project subclasses this and owns
/// its own migrations, because EF bakes provider-specific column types into them.
/// </summary>
public abstract class BlogKrafterDbContextBase(DbContextOptions options) : DbContext(options)
{
    /// <summary>
    /// Gets the categories.
    /// </summary>
    public DbSet<BlogCategory> Categories => Set<BlogCategory>();

    /// <summary>
    /// Gets the posts.
    /// </summary>
    public DbSet<BlogPost> Posts => Set<BlogPost>();

    /// <summary>
    /// Gets the post tags.
    /// </summary>
    public DbSet<BlogPostTag> PostTags => Set<BlogPostTag>();

    /// <summary>
    /// Gets the slug redirects.
    /// </summary>
    public DbSet<BlogSlugRedirect> SlugRedirects => Set<BlogSlugRedirect>();

    /// <summary>
    /// Gets the tags.
    /// </summary>
    public DbSet<BlogTag> Tags => Set<BlogTag>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(BlogKrafterDbContextBase).Assembly);
    }
}