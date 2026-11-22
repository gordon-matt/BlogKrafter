using BlogKrafter.Feeds;
using BlogKrafter.Posts;
using Microsoft.AspNetCore.Routing;

namespace BlogKrafter;

/// <summary>Maps the blog's RSS, Atom, and sitemap endpoints.</summary>
public static class EndpointRouteBuilderExtensions
{
    extension(IEndpointRouteBuilder endpoints)
    {
        /// <summary>
        /// Maps the RSS, Atom and sitemap documents under <see cref="BlogKrafterOptions.BasePath"/>.
        /// The pages themselves are Razor components: add <see cref="BlogKrafterComponents.Assembly"/>
        /// to the router and to <c>MapRazorComponents</c>, or host them on pages of your own.
        /// A document that can be built is returned as XML in UTF-8. A failed read is an HTTP 500 problem titled "The feed could not be built."
        /// The site address is <see cref="BlogKrafterOptions.SiteUrl"/>, or the request's scheme, host, and path base when that is blank.
        /// </summary>
        /// <returns>The <c>feeds</c> group, so the host can add its own endpoints beside them.</returns>
        public RouteGroupBuilder MapBlogKrafter()
        {
            var options = endpoints.ServiceProvider.GetRequiredService<IOptions<BlogKrafterOptions>>().Value;
            string basePath = string.IsNullOrEmpty(options.BasePath) ? "/" : options.BasePath;

            var group = endpoints.MapGroup(basePath + "/feeds").WithTags("BlogKrafter");

            group.MapGet("/rss.xml", async (HttpContext context, IBlogReader reader, IBlogFeedWriter writer) =>
            {
                var items = await reader.GetFeedItemsAsync(context.RequestAborted);
                return Xml(items, value => writer.WriteRss(value, SiteAddress.From(options, context.Request)), "application/rss+xml");
            });

            group.MapGet("/atom.xml", async (HttpContext context, IBlogReader reader, IBlogFeedWriter writer) =>
            {
                var items = await reader.GetFeedItemsAsync(context.RequestAborted);
                return Xml(items, value => writer.WriteAtom(value, SiteAddress.From(options, context.Request)), "application/atom+xml");
            });

            group.MapGet("/sitemap.xml", async (HttpContext context, IBlogReader reader, IBlogFeedWriter writer) =>
            {
                var entries = await reader.GetSitemapEntriesAsync(context.RequestAborted);
                return Xml(entries, value => writer.WriteSitemap(value, SiteAddress.From(options, context.Request)), "application/xml");
            });

            return group;
        }
    }

    private static Microsoft.AspNetCore.Http.IResult Xml<T>(Result<T> result, Func<T, string> write, string contentType) =>
        result.IsSuccess
            ? Results.Text(write(result.Value), contentType + "; charset=utf-8")
            : Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "The feed could not be built.");
}