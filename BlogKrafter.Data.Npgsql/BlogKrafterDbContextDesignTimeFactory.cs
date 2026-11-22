using Microsoft.EntityFrameworkCore.Design;

namespace BlogKrafter.Data.Npgsql;

/// <summary>Used by <c>dotnet ef</c>, so migrations can be generated without a host application.</summary>
public sealed class BlogKrafterDbContextDesignTimeFactory : IDesignTimeDbContextFactory<BlogKrafterDbContext>
{
    /// <summary>A context aimed at a local database named <c>blogkrafter_design</c> on the <c>postgres</c> user. The arguments are not used.</summary>
    /// <param name="args">The arguments <c>dotnet ef</c> passed. They are ignored.</param>
    /// <returns>The design-time context.</returns>
    public BlogKrafterDbContext CreateDbContext(string[] args) =>
        new(BlogKrafterDbContextFactory.BuildOptions(
            "Host=localhost;Database=blogkrafter_design;Username=postgres;Password=postgres"));
}