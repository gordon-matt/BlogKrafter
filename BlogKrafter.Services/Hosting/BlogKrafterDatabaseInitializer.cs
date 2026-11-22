using Microsoft.Extensions.Hosting;

namespace BlogKrafter.Hosting;

/// <summary>Applies BlogKrafter's migrations before the host starts serving requests.</summary>
public sealed class BlogKrafterDatabaseInitializer(
    ILogger<BlogKrafterDatabaseInitializer> logger,
    IServiceProvider services,
    IOptions<BlogKrafterOptions> options) : IHostedService
{
    /// <summary>
    /// Applies the migrations when <see cref="BlogKrafterOptions.AutoMigrate"/> is on.
    /// When it is off, the method returns without touching the database.
    /// </summary>
    /// <param name="cancellationToken">Cancels the migration.</param>
    /// <returns>A completed task once the database is current, or immediately when migration is off.</returns>
    /// <exception cref="InvalidOperationException">No storage provider was registered.</exception>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Resolved here rather than checked at registration so the provider call can come before or
        // after AddBlogKrafter.
        var contextFactory = services.GetService<IBlogKrafterDbContextFactory>()
            ?? throw new InvalidOperationException(
                "BlogKrafter has no storage provider. Call AddBlogKrafterSqlite, AddBlogKrafterSqlServer, AddBlogKrafterNpgsql or AddBlogKrafterMySql from the matching BlogKrafter.Data package.");

        if (!options.Value.AutoMigrate)
        {
            return;
        }

        await using var context = contextFactory.GetContext();
        await context.Database.MigrateAsync(cancellationToken);

        logger.LogInformation("BlogKrafter's {Provider} database is up to date", contextFactory.ProviderName);
    }

    /// <summary>Does nothing. Migration is finished in <see cref="StartAsync"/>.</summary>
    /// <param name="cancellationToken">Unused.</param>
    /// <returns>A completed task.</returns>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}