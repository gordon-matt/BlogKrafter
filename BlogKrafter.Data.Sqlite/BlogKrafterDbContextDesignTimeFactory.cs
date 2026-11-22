using Microsoft.EntityFrameworkCore.Design;

namespace BlogKrafter.Data.Sqlite;

/// <summary>Used by <c>dotnet ef</c>, so migrations can be generated without a host application.</summary>
public sealed class BlogKrafterDbContextDesignTimeFactory : IDesignTimeDbContextFactory<BlogKrafterDbContext>
{
    /// <summary>A context aimed at <c>blogkrafter_design.db</c> in the working directory. The arguments are not used.</summary>
    /// <param name="args">The arguments <c>dotnet ef</c> passed. They are ignored.</param>
    /// <returns>The design-time context.</returns>
    public BlogKrafterDbContext CreateDbContext(string[] args) =>
        new(BlogKrafterDbContextFactory.BuildOptions("Data Source=blogkrafter_design.db"));
}