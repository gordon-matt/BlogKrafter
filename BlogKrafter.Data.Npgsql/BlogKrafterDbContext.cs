namespace BlogKrafter.Data.Npgsql;

public sealed class BlogKrafterDbContext(DbContextOptions<BlogKrafterDbContext> options) : BlogKrafterDbContextBase(options);
