namespace BlogKrafter.Data.Sqlite;

/// <summary>
/// Represents the blog krafter database context.
/// </summary>
public sealed class BlogKrafterDbContext(DbContextOptions<BlogKrafterDbContext> options) : BlogKrafterDbContextBase(options)
{
    /// <inheritdoc/>
    protected override bool SupportsSchemas => false;
}
