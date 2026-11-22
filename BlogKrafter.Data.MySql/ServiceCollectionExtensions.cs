using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlogKrafter.Data.MySql;

/// <summary>Registers MySQL as BlogKrafter's storage.</summary>
public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Stores BlogKrafter's posts, categories and tags in MySQL.
        /// Replaces a factory already registered, so calling a different provider afterwards switches storage.
        /// Tables are prefixed because MySQL treats a schema as a database.
        /// </summary>
        /// <param name="connectionString">The MySQL connection string.</param>
        /// <returns>The same service collection.</returns>
        /// <exception cref="ArgumentException"><paramref name="connectionString"/> is null or white space.</exception>
        public IServiceCollection AddBlogKrafterMySql(string connectionString)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

            services.RemoveAll<IBlogKrafterDbContextFactory>();
            services.AddSingleton<IBlogKrafterDbContextFactory>(new BlogKrafterDbContextFactory(connectionString));

            return services;
        }
    }
}