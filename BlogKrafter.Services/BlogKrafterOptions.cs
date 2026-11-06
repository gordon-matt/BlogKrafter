namespace BlogKrafter;

/// <summary>
/// Bind from the <c>BlogKrafter</c> configuration section or set in code through <c>AddBlogKrafter</c>.
/// </summary>
public sealed class BlogKrafterOptions
{
    /// <summary>
    /// The section name constant.
    /// </summary>
    public const string SectionName = "BlogKrafter";

    /// <summary>
    /// The default base path constant.
    /// </summary>
    public const string DefaultBasePath = "/blog";

    /// <summary>
    /// The default admin path constant.
    /// </summary>
    public const string DefaultAdminPath = "/admin/blog";

    /// <summary>
    /// Where the public blog lives. Links, feeds, redirects and the sitemap all follow this. The pages
    /// BlogKrafter ships answer only at <see cref="DefaultBasePath"/>; any other value also needs the
    /// host page described in the README, because Blazor routes are fixed when the library is compiled.
    /// </summary>
    public string BasePath { get; set; } = DefaultBasePath;

    /// <summary>Where the editor screens live. A different value needs a host page, as for <see cref="BasePath"/>.</summary>
    public string AdminPath { get; set; } = DefaultAdminPath;

    /// <summary>The heading on the post list and the title of the feeds.</summary>
    public string Title { get; set; } = "Blog";

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The public address of the site, such as <c>https://example.com</c>. Feeds and canonical links
    /// need absolute URLs; when this is empty they are built from the incoming request.
    /// </summary>
    public string? SiteUrl { get; set; }

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Gets or sets the feed item count.
    /// </summary>
    public int FeedItemCount { get; set; } = 20;

    /// <summary>
    /// Gets or sets the related post count.
    /// </summary>
    public int RelatedPostCount { get; set; } = 3;

    /// <summary>Which editors authors can choose from.</summary>
    public BlogEditorMode EditorMode { get; set; } = BlogEditorMode.Both;

    /// <summary>The format a new post starts in when both are allowed.</summary>
    public ContentFormat DefaultContentFormat { get; set; } = ContentFormat.Markdown;

    /// <summary>
    /// Gets or sets the words per minute.
    /// </summary>
    public int WordsPerMinute { get; set; } = 220;

    /// <summary>A post gets a table of contents when it has at least this many second- and third-level headings.</summary>
    public int TableOfContentsMinHeadings { get; set; } = 3;

    /// <summary>A .NET date format for dates shown on the public pages.</summary>
    public string DateFormat { get; set; } = "MMMM d, yyyy";

    /// <summary>
    /// The time zone public dates are shown in, such as <c>Australia/Adelaide</c>. Public pages are
    /// rendered on the server and cannot see a reader's clock, so without this a post published in the
    /// morning east of Greenwich shows the previous day's date. Empty means UTC.
    /// </summary>
    public string? TimeZone { get; set; }

    /// <summary>
    /// Roles allowed into the editor screens. Empty lets any signed-in user in. Ignored when the host
    /// registers its own policy named <c>BlogKrafter.Manage</c>.
    /// </summary>
    public IList<string> ManagerRoles { get; set; } = ["Administrator"];

    /// <summary>
    /// Hosts allowed in an embedded <c>iframe</c> (video players, for example). Any other frame is removed
    /// when a post is saved.
    /// </summary>
    public IList<string> AllowedFrameHosts { get; set; } =
    [
        "www.youtube.com",
        "www.youtube-nocookie.com",
        "player.vimeo.com",
    ];

    /// <summary>Applies BlogKrafter's migrations at startup. Turn off when schema changes go through a pipeline.</summary>
    public bool AutoMigrate { get; set; } = true;

    /// <summary>
    /// Layout for the public pages BlogKrafter ships. Usually the host's own public layout. When null the
    /// NetForge shell layout is used.
    /// </summary>
    public Type? PublicLayout { get; set; }

    /// <summary>Layout for the editor screens. When null the NetForge shell layout is used.</summary>
    public Type? AdminLayout { get; set; }

    /// <summary>
    /// A component rendered under each post that allows comments, such as a Giscus or Disqus embed. It
    /// receives the post as a <c>Post</c> parameter of type <c>BlogKrafter.Models.PostDto</c>.
    /// </summary>
    public Type? CommentsComponent { get; set; }
}

/// <summary>
/// Specifies the blog editor mode.
/// </summary>
public enum BlogEditorMode
{
    /// <summary>
    /// The markdown value.
    /// </summary>
    Markdown,

    /// <summary>Requires a Pickstack rich text editor to be registered.</summary>
    RichText,

    /// <summary>Authors choose per post. Without a registered rich text editor this behaves like <see cref="Markdown"/>.</summary>
    Both,
}