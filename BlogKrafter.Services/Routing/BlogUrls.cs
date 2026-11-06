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

    /// <summary>
    /// Gets the base path.
    /// </summary>
    public string BasePath => options.Value.BasePath;

    /// <summary>
    /// Gets the admin path.
    /// </summary>
    public string AdminPath => options.Value.AdminPath;

    /// <summary>
    /// Gets the index.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <param name="search">The search.</param>
    /// <returns>The index.</returns>
    public string Index(int page = 1, string? search = null)
    {
        string path = BasePath.Length == 0 ? "/" : BasePath;
        return WithQuery(path, page, search);
    }

    /// <summary>
    /// Gets the post.
    /// </summary>
    /// <param name="slug">The slug.</param>
    /// <returns>The post.</returns>
    public string Post(string slug) => $"{BasePath}/{Uri.EscapeDataString(slug)}";

    /// <summary>The saved post as an editor sees it, including drafts and scheduled posts.</summary>
    public string Preview(string slug) => $"{Post(slug)}?preview=true";

    /// <summary>
    /// Gets the tag.
    /// </summary>
    /// <param name="slug">The slug.</param>
    /// <param name="page">The page.</param>
    /// <returns>The tag.</returns>
    public string Tag(string slug, int page = 1) => WithQuery($"{BasePath}/tag/{Uri.EscapeDataString(slug)}", page, null);

    /// <summary>
    /// Gets the category.
    /// </summary>
    /// <param name="slug">The slug.</param>
    /// <param name="page">The page.</param>
    /// <returns>The category.</returns>
    public string Category(string slug, int page = 1) => WithQuery($"{BasePath}/category/{Uri.EscapeDataString(slug)}", page, null);

    /// <summary>
    /// Archives the value.
    /// </summary>
    /// <returns>The string.</returns>
    public string Archive() => $"{BasePath}/archive";

    // Two segments under the base path, so the interactive router finds no page for them and lets the
    // browser load the endpoint, instead of treating "rss.xml" as a post slug.
    /// <summary>
    /// Gets the rss.
    /// </summary>
    /// <returns>The rss.</returns>
    public string Rss() => $"{BasePath}/feeds/rss.xml";

    /// <summary>
    /// Gets the atom.
    /// </summary>
    /// <returns>The atom.</returns>
    public string Atom() => $"{BasePath}/feeds/atom.xml";

    /// <summary>
    /// Gets the sitemap.
    /// </summary>
    /// <returns>The sitemap.</returns>
    public string Sitemap() => $"{BasePath}/feeds/sitemap.xml";

    /// <summary>
    /// Gets the admin.
    /// </summary>
    /// <returns>The admin.</returns>
    public string Admin() => AdminPath;

    /// <summary>
    /// Gets the admin new post.
    /// </summary>
    /// <returns>The admin new post.</returns>
    public string AdminNewPost() => $"{AdminPath}/posts/new";

    /// <summary>
    /// Gets the admin edit post.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns>The admin edit post.</returns>
    public string AdminEditPost(int id) => $"{AdminPath}/posts/{id.ToString(CultureInfo.InvariantCulture)}";

    /// <summary>
    /// Gets the admin categories.
    /// </summary>
    /// <returns>The admin categories.</returns>
    public string AdminCategories() => $"{AdminPath}/categories";

    /// <summary>
    /// Gets the admin tags.
    /// </summary>
    /// <returns>The admin tags.</returns>
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