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

    public string BasePath => options.Value.BasePath;

    public string AdminPath => options.Value.AdminPath;

    public string Index(int page = 1, string? search = null)
    {
        string path = BasePath.Length == 0 ? "/" : BasePath;
        return WithQuery(path, page, search);
    }

    public string Post(string slug) => $"{BasePath}/{Uri.EscapeDataString(slug)}";

    /// <summary>The saved post as an editor sees it, including drafts and scheduled posts.</summary>
    public string Preview(string slug) => $"{Post(slug)}?preview=true";

    public string Tag(string slug, int page = 1) => WithQuery($"{BasePath}/tag/{Uri.EscapeDataString(slug)}", page, null);

    public string Category(string slug, int page = 1) => WithQuery($"{BasePath}/category/{Uri.EscapeDataString(slug)}", page, null);

    public string Archive() => $"{BasePath}/archive";

    // Two segments under the base path, so the interactive router finds no page for them and lets the
    // browser load the endpoint, instead of treating "rss.xml" as a post slug.
    public string Rss() => $"{BasePath}/feeds/rss.xml";

    public string Atom() => $"{BasePath}/feeds/atom.xml";

    public string Sitemap() => $"{BasePath}/feeds/sitemap.xml";

    public string Admin() => AdminPath;

    public string AdminNewPost() => $"{AdminPath}/posts/new";

    public string AdminEditPost(int id) => $"{AdminPath}/posts/{id.ToString(CultureInfo.InvariantCulture)}";

    public string AdminCategories() => $"{AdminPath}/categories";

    public string AdminTags() => $"{AdminPath}/tags";

    /// <summary>Joins <paramref name="siteUrl"/> (scheme, host and any path base) to a root-relative path.</summary>
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