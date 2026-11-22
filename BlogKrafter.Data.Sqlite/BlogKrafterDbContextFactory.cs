using Microsoft.Data.Sqlite;

namespace BlogKrafter.Data.Sqlite;

/// <summary>Opens SQLite contexts. Each connection turns foreign keys on and creates the database folder when it is missing.</summary>
public sealed class BlogKrafterDbContextFactory(string connectionString) : IBlogKrafterDbContextFactory
{
    private DbContextOptions<BlogKrafterDbContext> Options => field ??= BuildOptions(connectionString);

    /// <summary>The value is <c>Sqlite</c>.</summary>
    public string ProviderName => Constants.DatabaseProviders.Sqlite;

    /// <summary>A context for the connection string this factory was created with.</summary>
    /// <returns>A new context. The caller disposes it.</returns>
    public DbContext GetContext() => new BlogKrafterDbContext(Options);

    /// <summary>A context for a different connection string. The factory's own string is left unchanged.</summary>
    /// <param name="connectionString">The SQLite connection string.</param>
    /// <returns>A new context. The caller disposes it.</returns>
    public DbContext GetContext(string connectionString) => new BlogKrafterDbContext(BuildOptions(connectionString));

    internal static DbContextOptions<BlogKrafterDbContext> BuildOptions(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BlogKrafterDbContext>();

        optionsBuilder.UseSqlite(Prepare(connectionString), sqlite => sqlite
            .MigrationsAssembly(typeof(BlogKrafterDbContext).Assembly.GetName().Name)
            .MigrationsHistoryTable(Constants.MigrationsHistoryTable));

        return optionsBuilder.Options;
    }

    /// <summary>
    /// Creates the database's folder, since SQLite creates a missing file but not a missing directory,
    /// and turns foreign keys on so deleting a post removes its tag links and redirects.
    /// </summary>
    private static string Prepare(string connectionString)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString) { ForeignKeys = true };

        if (builder.DataSource is { Length: > 0 } dataSource
            && !dataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase)
            && builder.Mode != SqliteOpenMode.Memory
            && Path.GetDirectoryName(Path.GetFullPath(dataSource)) is { Length: > 0 } directory)
        {
            Directory.CreateDirectory(directory);
        }

        return builder.ToString();
    }
}