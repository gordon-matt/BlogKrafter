using Microsoft.EntityFrameworkCore.Design;

namespace BlogKrafter.Data.MySql;

/// <summary>Used by <c>dotnet ef</c>, so migrations can be generated without a host application.</summary>
public sealed class BlogKrafterDbContextDesignTimeFactory : IDesignTimeDbContextFactory<BlogKrafterDbContext>
{
    public BlogKrafterDbContext CreateDbContext(string[] args) =>
        new(BlogKrafterDbContextFactory.BuildOptions(
            "Server=localhost;Database=blogkrafter_design;User ID=root;Password=root"));
}