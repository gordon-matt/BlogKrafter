namespace BlogKrafter.Data;

/// <summary>
/// Provider-agnostic context. Each <c>BlogKrafter.Data.{Provider}</c> project subclasses this and owns
/// its own migrations, because EF bakes provider-specific column types into them.
/// </summary>
public abstract class BlogKrafterDbContextBase(DbContextOptions options) : DbContext(options)
{
    public DbSet<BlogCategory> Categories => Set<BlogCategory>();

    public DbSet<BlogPost> Posts => Set<BlogPost>();

    public DbSet<BlogPostTag> PostTags => Set<BlogPostTag>();

    public DbSet<BlogSlugRedirect> SlugRedirects => Set<BlogSlugRedirect>();

    public DbSet<BlogTag> Tags => Set<BlogTag>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(BlogKrafterDbContextBase).Assembly);
    }
}