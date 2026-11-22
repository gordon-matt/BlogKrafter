namespace BlogKrafter.Data.Sql;

/// <summary>The SQL Server context. Tables live in the <c>bkraft</c> schema, and migrations are stored in BlogKrafter's own history table.</summary>
public sealed class BlogKrafterDbContext(DbContextOptions<BlogKrafterDbContext> options) : BlogKrafterDbContextBase(options);