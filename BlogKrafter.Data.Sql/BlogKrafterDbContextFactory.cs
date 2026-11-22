namespace BlogKrafter.Data.Sql;

/// <summary>Opens SQL Server contexts for the connection string given at registration.</summary>
public sealed class BlogKrafterDbContextFactory(string connectionString) : IBlogKrafterDbContextFactory
{
    private DbContextOptions<BlogKrafterDbContext> Options => field ??= BuildOptions(connectionString);

    /// <summary>The value is <c>SqlServer</c>.</summary>
    public string ProviderName => Constants.DatabaseProviders.SqlServer;

    /// <summary>A context for the connection string this factory was created with.</summary>
    /// <returns>A new context. The caller disposes it.</returns>
    public DbContext GetContext() => new BlogKrafterDbContext(Options);

    /// <summary>A context for a different connection string. The factory's own string is left unchanged.</summary>
    /// <param name="connectionString">The SQL Server connection string.</param>
    /// <returns>A new context. The caller disposes it.</returns>
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