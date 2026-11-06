namespace BlogKrafter.Data.MySql;

/// <summary>
/// Represents the blog krafter database context.
/// </summary>
public sealed class BlogKrafterDbContext(DbContextOptions<BlogKrafterDbContext> options) : BlogKrafterDbContextBase(options);