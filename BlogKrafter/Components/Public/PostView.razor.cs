using System.Globalization;
using System.Text.Json;
using BlogKrafter.Posts;
using Heroicsoft.NetForge.Results;

namespace BlogKrafter.Components.Public;

/// <summary>
/// Represents the post view.
/// </summary>
public partial class PostView
{
    private PostDto? post;
    private string? loadError;
    private bool notFound;

    /// <summary>
    /// Gets or sets the slug.
    /// </summary>
    [Parameter, EditorRequired]
    public string Slug { get; set; } = string.Empty;

    /// <summary>Shows drafts and scheduled posts. The caller decides who may set it.</summary>
    [Parameter]
    public bool Preview { get; set; }

    [Inject]
    private IBlogReader Reader { get; set; } = default!;

    [Inject]
    private IOptions<BlogKrafterOptions> Options { get; set; } = default!;

    [Inject]
    private BlogUrls Urls { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private string SiteUrl => SiteAddress.From(Options.Value, Navigation);

    private string CanonicalUrl => post?.CanonicalUrl ?? BlogUrls.Absolute(SiteUrl, Urls.Post(post?.Slug ?? Slug));

    private string? CoverImageUrl =>
        string.IsNullOrWhiteSpace(post?.CoverImageUrl) ? null : BlogUrls.Absolute(SiteUrl, post.CoverImageUrl);

    private string Description => post?.MetaDescription ?? post?.Excerpt ?? string.Empty;

    private Dictionary<string, object?> CommentParameters => new() { ["Post"] = post };

    /// <summary>
    /// schema.org data for search results, written as markup because components may not contain script
    /// elements. The serializer escapes <c>&lt;</c>, so post text cannot close the element.
    /// </summary>
    private string StructuredData
    {
        get
        {
            if (post is null)
            {
                return string.Empty;
            }

            var data = new Dictionary<string, object?>
            {
                ["@context"] = "https://schema.org",
                ["@type"] = "BlogPosting",
                ["headline"] = post.Title,
                ["description"] = Description,
                ["url"] = CanonicalUrl,
                ["mainEntityOfPage"] = CanonicalUrl,
                ["datePublished"] = post.PublishedAt is DateTime published ? IsoDate(published) : null,
                ["dateModified"] = IsoDate(post.UpdatedAt),
                ["author"] = new Dictionary<string, string> { ["@type"] = "Person", ["name"] = post.AuthorName },
                ["image"] = CoverImageUrl,
                ["keywords"] = post.Tags.Count == 0 ? null : string.Join(", ", post.Tags.Select(tag => tag.Name)),
            };

            string json = JsonSerializer.Serialize(data.Where(pair => pair.Value is not null).ToDictionary());
            return $"<script type=\"application/ld+json\">{json}</script>";
        }
    }

    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync()
    {
        loadError = null;
        notFound = false;

        var found = await Reader.GetPostAsync(Slug, Preview);
        if (found.IsSuccess && found.Value.RedirectSlug is string current)
        {
            string query = Preview ? "?preview=true" : string.Empty;
            Navigation.NavigateTo(Urls.Post(current) + query, replace: true);
            return;
        }

        if (found.IsSuccess)
        {
            post = found.Value.Post;
            return;
        }

        post = null;
        if (found.Status is ResultStatus.NotFound or ResultStatus.Forbidden)
        {
            notFound = true;
            Navigation.NotFound();
            return;
        }

        loadError = found.GetDisplayMessage();
    }

    /// <summary>
    /// The full path plus the fragment: with a <c>base</c> element, a bare <c>#id</c> would resolve
    /// against the site root and leave the post.
    /// </summary>
    private string Anchor(string id) => $"{(Preview ? Urls.Preview(post!.Slug) : Urls.Post(post!.Slug))}#{id}";

    private static string IsoDate(DateTime utc) =>
        DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
}