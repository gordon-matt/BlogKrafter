using System.Globalization;

namespace BlogKrafter.Routing;

/// <summary>
/// Every address BlogKrafter links to, built from <see cref="BlogKrafterOptions.BasePath"/> and
/// <see cref="BlogKrafterOptions.AdminPath"/> so a host that moves the blog does not leave stale links.
/// </summary>
public sealed class BlogUrls(IOptions<BlogKrafterOptions> options)
{
    /// <summary>
    /// Slugs the public routes already use. A post with one of these slugs could never be reached,
    /// so they are refused when a post is saved.
    /// </summary>
    public static IReadOnlySet<string> ReservedSlugs { get; } =
        new HashSet<string>(["archive", "category", "feeds", "page", "tag"], StringComparer.Ordinal);

    /// <summary>The public path, such as <c>/blog</c>. Empty when the blog is the site root.</summary>
    public string BasePath => options.Value.BasePath;

    /// <summary>The editor path, such as <c>/admin/blog</c>.</summary>
    public string AdminPath => options.Value.AdminPath;

    /// <summary>
    /// The blog index. Page 1 is omitted from the query, and a search is added as <c>q</c>.
    /// An empty base path is <c>/</c>.
    /// </summary>
    /// <param name="page">The page. 1 leaves the query without a page.</param>
    /// <param name="search">The search text. Blank is omitted.</param>
    /// <returns>The root-relative index address.</returns>
    public string Index(int page = 1, string? search = null)
    {
        string path = BasePath.Length == 0 ? "/" : BasePath;
        return WithQuery(path, page, search);
    }

    /// <summary>The post's address. The slug is escaped.</summary>
    /// <param name="slug">The address segment.</param>
    /// <returns>The root-relative post address.</returns>
    public string Post(string slug) => $"{BasePath}/{Uri.EscapeDataString(slug)}";

    /// <summary>The saved post as an editor sees it, including drafts and scheduled posts.</summary>
    public string Preview(string slug) => $"{Post(slug)}?preview=true";

    /// <summary>The tag page. Page 1 is omitted from the query. The slug is escaped.</summary>
    /// <param name="slug">The tag's address segment.</param>
    /// <param name="page">The page. 1 leaves the query without a page.</param>
    /// <returns>The root-relative tag address.</returns>
    public string Tag(string slug, int page = 1) => WithQuery($"{BasePath}/tag/{Uri.EscapeDataString(slug)}", page, null);

    /// <summary>The category page. Page 1 is omitted from the query. The slug is escaped.</summary>
    /// <param name="slug">The category's address segment.</param>
    /// <param name="page">The page. 1 leaves the query without a page.</param>
    /// <returns>The root-relative category address.</returns>
    public string Category(string slug, int page = 1) => WithQuery($"{BasePath}/category/{Uri.EscapeDataString(slug)}", page, null);

    /// <summary>The archive page, under <c>archive</c>.</summary>
    /// <returns>The root-relative archive address.</returns>
    public string Archive() => $"{BasePath}/archive";

    // Two segments under the base path, so the interactive router finds no page for them and lets the
    // browser load the endpoint, instead of treating "rss.xml" as a post slug.
    /// <summary>The RSS feed, under <c>feeds/rss.xml</c> so the page router does not treat the file name as a post.</summary>
    /// <returns>The root-relative RSS address.</returns>
    public string Rss() => $"{BasePath}/feeds/rss.xml";

    /// <summary>The Atom feed, under <c>feeds/atom.xml</c>.</summary>
    /// <returns>The root-relative Atom address.</returns>
    public string Atom() => $"{BasePath}/feeds/atom.xml";

    /// <summary>The sitemap, under <c>feeds/sitemap.xml</c>.</summary>
    /// <returns>The root-relative sitemap address.</returns>
    public string Sitemap() => $"{BasePath}/feeds/sitemap.xml";

    /// <summary>The editor's post list.</summary>
    /// <returns>The editor path.</returns>
    public string Admin() => AdminPath;

    /// <summary>The new-post screen, under <c>posts/new</c>.</summary>
    /// <returns>The root-relative editor address.</returns>
    public string AdminNewPost() => $"{AdminPath}/posts/new";

    /// <summary>The edit screen for one post. The id is written with invariant digits.</summary>
    /// <param name="id">The post id.</param>
    /// <returns>The root-relative editor address.</returns>
    public string AdminEditPost(int id) => $"{AdminPath}/posts/{id.ToString(CultureInfo.InvariantCulture)}";

    /// <summary>The category screen, under <c>categories</c>.</summary>
    /// <returns>The root-relative editor address.</returns>
    public string AdminCategories() => $"{AdminPath}/categories";

    /// <summary>The tag screen, under <c>tags</c>.</summary>
    /// <returns>The root-relative editor address.</returns>
    public string AdminTags() => $"{AdminPath}/tags";

    /// <summary>
    /// Joins <paramref name="siteUrl"/> (scheme, host and any path base) to a root-relative path.
    /// A path that is already an absolute http(s) address is returned unchanged.
    /// </summary>
    /// <param name="siteUrl">Scheme, host and any path base.</param>
    /// <param name="path">A root-relative path, or an absolute http(s) address.</param>
    /// <returns>The absolute address.</returns>
    public static string Absolute(string siteUrl, string path) => Uri.TryCreate(path, UriKind.Absolute, out var absolute) && absolute.Scheme is "http" or "https"
            ? path
            : siteUrl.TrimEnd('/') + "/" + path.TrimStart('/');

    internal static string NormalizePath(string? path, string fallback)
    {
        string trimmed = (path ?? string.Empty).Trim().Trim('/');
        return trimmed.Length == 0 ? string.IsNullOrWhiteSpace(path) ? fallback : string.Empty : "/" + trimmed;
    }

    private static string WithQuery(string path, int page, string? search)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query.Add("q=" + Uri.EscapeDataString(search.Trim()));
        }

        if (page > 1)
        {
            query.Add("page=" + page.ToString(CultureInfo.InvariantCulture));
        }

        return query.Count == 0 ? path : path + "?" + string.Join('&', query);
    }
}