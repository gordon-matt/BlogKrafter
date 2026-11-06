namespace BlogKrafter.Data.Sql;

/// <summary>
/// Represents the blog krafter database context factory.
/// </summary>
public sealed class BlogKrafterDbContextFactory(string connectionString) : IBlogKrafterDbContextFactory
{
    private DbContextOptions<BlogKrafterDbContext> Options => field ??= BuildOptions(connectionString);

    /// <summary>
    /// Gets the provider name.
    /// </summary>
    public string ProviderName => Constants.DatabaseProviders.SqlServer;

    /// <summary>
    /// Gets the context.
    /// </summary>
    /// <returns>The context.</returns>
    public DbContext GetContext() => new BlogKrafterDbContext(Options);

    /// <summary>
    /// Gets the context.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>The context.</returns>
    public DbContext GetContext(string connectionString) => new BlogKrafterDbContext(BuildOptions(connectionString));

    internal static DbContextOptions<BlogKrafterDbContext> BuildOptions(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BlogKrafterDbContext>();

        optionsBuilder.UseSqlServer(connectionString, sql => sql
            .MigrationsAssembly(typeof(BlogKrafterDbContext).Assembly.GetName().Name)
            .MigrationsHistoryTable(Constants.MigrationsHistoryTable, Constants.Schema));

        return optionsBuilder.Options;
    }
}