using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlogKrafter.Data.Npgsql;

/// <summary>Registers PostgreSQL as BlogKrafter's storage.</summary>
public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Stores BlogKrafter's posts, categories and tags in PostgreSQL.
        /// Replaces a factory already registered, so calling a different provider afterwards switches storage.
        /// </summary>
        /// <param name="connectionString">The PostgreSQL connection string.</param>
        /// <returns>The same service collection.</returns>
        /// <exception cref="ArgumentException"><paramref name="connectionString"/> is null or white space.</exception>
        public IServiceCollection AddBlogKrafterNpgsql(string connectionString)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

            services.RemoveAll<IBlogKrafterDbContextFactory>();
            services.AddSingleton<IBlogKrafterDbContextFactory>(new BlogKrafterDbContextFactory(connectionString));

            return services;
        }
    }
}