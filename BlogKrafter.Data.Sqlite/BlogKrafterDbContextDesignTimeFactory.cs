using Microsoft.EntityFrameworkCore.Design;

namespace BlogKrafter.Data.Sqlite;

/// <summary>Used by <c>dotnet ef</c>, so migrations can be generated without a host application.</summary>
public sealed class BlogKrafterDbContextDesignTimeFactory : IDesignTimeDbContextFactory<BlogKrafterDbContext>
{
    /// <summary>
    /// Creates the database context.
    /// </summary>
    /// <param name="args">The args.</param>
    /// <returns>The database context.</returns>
    public BlogKrafterDbContext CreateDbContext(string[] args) =>
        new(BlogKrafterDbContextFactory.BuildOptions("Data Source=blogkrafter_design.db"));
}