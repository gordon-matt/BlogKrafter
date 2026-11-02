using BlogKrafter.Feeds;
using BlogKrafter.Hosting;
using BlogKrafter.Taxonomy;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlogKrafter;

public static class BlogKrafterServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers BlogKrafter's services without any UI. A storage provider must be added as well,
        /// from one of the <c>BlogKrafter.Data.*</c> packages. Most hosts call <c>AddBlogKrafter</c>
        /// from the <c>BlogKrafter</c> package instead, which calls this.
        /// </summary>
        public IServiceCollection AddBlogKrafterServices(Action<BlogKrafterOptions>? configure = null)
        {
            services.AddOptions<BlogKrafterOptions>()
                .Configure(options => configure?.Invoke(options))
                .PostConfigure(options =>
                {
                    options.BasePath = BlogUrls.NormalizePath(options.BasePath, BlogKrafterOptions.DefaultBasePath);
                    options.AdminPath = BlogUrls.NormalizePath(options.AdminPath, BlogKrafterOptions.DefaultAdminPath);
                    options.SiteUrl = string.IsNullOrWhiteSpace(options.SiteUrl) ? null : options.SiteUrl.TrimEnd('/');
                })
                .Validate(
                    options => string.IsNullOrWhiteSpace(options.TimeZone) || TimeZoneInfo.TryFindSystemTimeZoneById(options.TimeZone, out _),
                    "BlogKrafter's TimeZone is not a time zone known to this server.")
                .ValidateOnStart();

            services.TryAddSingleton(TimeProvider.System);
            services.TryAddSingleton<BlogTimeZone>();
            services.TryAddScoped<IBlogTenantProvider, SingleTenantProvider>();
            services.TryAddSingleton<BlogUrls>();
            services.TryAddSingleton<IBlogHtmlSanitizer, BlogHtmlSanitizer>();
            services.TryAddSingleton<IBlogContentRenderer, BlogContentRenderer>();
            services.TryAddSingleton<IBlogFeedWriter, BlogFeedWriter>();

            // Closed registrations against BlogKrafter's own factory. A host built on Extenso maps the
            // open IRepository<> to its own IDbContextFactory, and closed registrations win, so the
            // two never cross.
            services.TryAddScoped(BlogRepository<BlogPost>);
            services.TryAddScoped(BlogRepository<BlogPostTag>);
            services.TryAddScoped(BlogRepository<BlogCategory>);
            services.TryAddScoped(BlogRepository<BlogTag>);
            services.TryAddScoped(BlogRepository<BlogSlugRedirect>);

            services.TryAddScoped<IBlogReader, BlogReader>();
            services.TryAddScoped<IBlogPostService, BlogPostService>();
            services.TryAddScoped<IBlogCategoryService, BlogCategoryService>();
            services.TryAddScoped<IBlogTagService, BlogTagService>();

            services.AddHostedService<BlogKrafterDatabaseInitializer>();

            return services;
        }
    }

    private static IRepository<TEntity> BlogRepository<TEntity>(IServiceProvider provider)
        where TEntity : class, IEntity =>
        new EntityFrameworkRepository<TEntity>(provider.GetRequiredService<IBlogKrafterDbContextFactory>());
}
