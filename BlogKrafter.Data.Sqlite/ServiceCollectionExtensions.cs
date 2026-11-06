using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlogKrafter.Data.Sqlite;

/// <summary>
/// Provides extension methods for service collections.
/// </summary>
public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>Stores BlogKrafter's posts, categories and tags in SQLite.</summary>
        /// <param name="connectionString">
        /// A SQLite connection string. When omitted, the blog goes to <c>BlogKrafter.db</c> under the
        /// user's local application data folder, which survives redeploying the app's own folder.
        /// </param>
        public IServiceCollection AddBlogKrafterSqlite(string? connectionString = null)
        {
            string resolved = string.IsNullOrWhiteSpace(connectionString)
                ? $"Data Source={DefaultDatabasePath()}"
                : connectionString;

            services.RemoveAll<IBlogKrafterDbContextFactory>();
            services.AddSingleton<IBlogKrafterDbContextFactory>(new BlogKrafterDbContextFactory(resolved));

            return services;
        }
    }

    private static string DefaultDatabasePath()
    {
        string appName = Assembly.GetEntryAssembly()?.GetName().Name ?? "BlogKrafter";

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            appName,
            "BlogKrafter.db");
    }
}