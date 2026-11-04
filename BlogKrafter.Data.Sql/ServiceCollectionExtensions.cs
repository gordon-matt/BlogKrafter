using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlogKrafter.Data.Sql;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>Stores BlogKrafter's posts, categories and tags in SQL Server.</summary>
        public IServiceCollection AddBlogKrafterSqlServer(string connectionString)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

            services.RemoveAll<IBlogKrafterDbContextFactory>();
            services.AddSingleton<IBlogKrafterDbContextFactory>(new BlogKrafterDbContextFactory(connectionString));

            return services;
        }
    }
}