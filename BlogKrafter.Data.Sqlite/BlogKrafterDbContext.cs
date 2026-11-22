namespace BlogKrafter.Data.Sqlite;

/// <summary>The SQLite context. Tables are prefixed with <c>BlogKrafter_</c> because SQLite has no schemas.</summary>
public sealed class BlogKrafterDbContext(DbContextOptions<BlogKrafterDbContext> options) : BlogKrafterDbContextBase(options)
{
    /// <inheritdoc/>
    protected override bool SupportsSchemas => false;
}
