namespace BlogKrafter.Data.MySql;

/// <summary>The MySQL context. Tables are prefixed with <c>BlogKrafter_</c> because a schema is a database in MySQL.</summary>
public sealed class BlogKrafterDbContext(DbContextOptions<BlogKrafterDbContext> options) : BlogKrafterDbContextBase(options)
{
    /// <inheritdoc/>
    protected override bool SupportsSchemas => false;
}
