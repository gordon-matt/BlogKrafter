using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlogKrafter.Data.Npgsql;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>Stores BlogKrafter's posts, categories and tags in PostgreSQL.</summary>
        public IServiceCollection AddBlogKrafterNpgsql(string connectionString)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

            services.RemoveAll<IBlogKrafterDbContextFactory>();
            services.AddSingleton<IBlogKrafterDbContextFactory>(new BlogKrafterDbContextFactory(connectionString));

            return services;
        }
    }
}