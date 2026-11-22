namespace BlogKrafter.Data.MySql;

/// <summary>Opens MySQL contexts for the connection string given at registration.</summary>
public sealed class BlogKrafterDbContextFactory(string connectionString) : IBlogKrafterDbContextFactory
{
    private DbContextOptions<BlogKrafterDbContext> Options => field ??= BuildOptions(connectionString);

    /// <summary>The value is <c>MySql</c>.</summary>
    public string ProviderName => Constants.DatabaseProviders.MySql;

    /// <summary>A context for the connection string this factory was created with.</summary>
    /// <returns>A new context. The caller disposes it.</returns>
    public DbContext GetContext() => new BlogKrafterDbContext(Options);

    /// <summary>A context for a different connection string. The factory's own string is left unchanged.</summary>
    /// <param name="connectionString">The MySQL connection string.</param>
    /// <returns>A new context. The caller disposes it.</returns>
    public DbContext GetContext(string connectionString) => new BlogKrafterDbContext(BuildOptions(connectionString));

    internal static DbContextOptions<BlogKrafterDbContext> BuildOptions(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BlogKrafterDbContext>();

        optionsBuilder.UseMySQL(connectionString, mySql => mySql
            .MigrationsAssembly(typeof(BlogKrafterDbContext).Assembly.GetName().Name)
            .MigrationsHistoryTable(Constants.MigrationsHistoryTable));

        return optionsBuilder.Options;
    }
}