using Microsoft.EntityFrameworkCore.Design;

namespace BlogKrafter.Data.Npgsql;

/// <summary>Used by <c>dotnet ef</c>, so migrations can be generated without a host application.</summary>
public sealed class BlogKrafterDbContextDesignTimeFactory : IDesignTimeDbContextFactory<BlogKrafterDbContext>
{
    public BlogKrafterDbContext CreateDbContext(string[] args) =>
        new(BlogKrafterDbContextFactory.BuildOptions(
            "Host=localhost;Database=blogkrafter_design;Username=postgres;Password=postgres"));
}
