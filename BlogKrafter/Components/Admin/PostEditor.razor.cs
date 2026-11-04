using System.Globalization;
using BlogKrafter.Content;
using BlogKrafter.Posts;
using BlogKrafter.Taxonomy;
using Heroicsoft.NetForge.Components.Dialogs;
using Heroicsoft.NetForge.Components.Toasts;
using Heroicsoft.NetForge.Results;

namespace BlogKrafter.Components.Admin;

public partial class PostEditor
{
    private const string LocalInputFormat = "yyyy-MM-dd'T'HH:mm";

    private SavePostRequest model = new();
    private PostEditDto? saved;
    private IReadOnlyList<CategoryDto> categories = [];
    private List<TagChoice> tags = [];
    private Dictionary<string, string> errors = new(StringComparer.Ordinal);
    private BrowserTime time = BrowserTime.Utc;
    private string? publishLocal;
    private bool loading = true;
    private string? loadError;
    private bool saving;
    private bool showDetails;

    /// <summary>Null for a new post.</summary>
    [Parameter]
    public int? PostId { get; set; }

    [Inject]
    private IBlogPostService Posts { get; set; } = default!;

    [Inject]
    private IBlogCategoryService Categories { get; set; } = default!;

    [Inject]
    private IBlogTagService Tags { get; set; } = default!;

    [Inject]
    private EditorCapabilities Capabilities { get; set; } = default!;

    [Inject]
    private IOptions<BlogKrafterOptions> Options { get; set; } = default!;

    [Inject]
    private BlogUrls Urls { get; set; } = default!;

    [Inject]
    private BlogScript Script { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private DialogService Dialogs { get; set; } = default!;

    [Inject]
    private ToastService Toasts { get; set; } = default!;

    [Inject]
    private TimeProvider Clock { get; set; } = default!;

    private bool IsNew => PostId is null;

    private string SaveLabel => (model.Status, IsScheduled, CurrentState) switch
    {
        (PostStatus.Draft, _, _) => "Save draft",
        (_, true, _) => "Schedule",
        (_, _, PostState.Published) => "Update",
        _ => "Publish",
    };

    private bool IsScheduled => model.PublishedAt is DateTime at && at > Clock.GetUtcNow().UtcDateTime;

    private PostState CurrentState => saved?.State ?? PostState.Draft;

    private string PreviewHref => saved is null
        ? Urls.Index()
        : saved.State == PostState.Published ? Urls.Post(saved.Slug) : Urls.Preview(saved.Slug);

    /// <summary>
    /// The address the post will have, as the service will derive it. A title already used by another
    /// post gets a number added on save, which this cannot know in advance.
    /// </summary>
    private string AddressPreview =>
        Slugs.Generate(string.IsNullOrWhiteSpace(model.Slug) ? model.Title : model.Slug) is { Length: > 0 } slug
            ? Urls.Post(slug)
            : $"{Urls.BasePath}/your-post-title";

    private string PublishHint => (model.Status, model.PublishedAt) switch
    {
        (PostStatus.Draft, _) => "Drafts are never shown to readers.",
        (_, null) => "Goes live as soon as it is saved.",
        _ when IsScheduled => $"Goes live {FormatLocal(model.PublishedAt!.Value)}.",
        _ => $"Live since {FormatLocal(model.PublishedAt!.Value)}.",
    };

    protected override async Task OnInitializedAsync()
    {
        var foundCategories = await Categories.ListAsync();
        categories = foundCategories.IsSuccess ? foundCategories.Value : [];

        if (PostId is not int id)
        {
            model = new SavePostRequest { ContentFormat = Capabilities.DefaultFormat };
            loading = false;
            return;
        }

        var found = await Posts.GetAsync(id);
        if (!found.IsSuccess)
        {
            loadError = found.GetDisplayMessage("There is no post with that id. It may have been deleted.");
            loading = false;
            return;
        }

        Load(found.Value);
        loading = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        time = BrowserTime.From(await Script.TimeZoneAsync());
        publishLocal = ToLocalInput(model.PublishedAt);
        StateHasChanged();
    }

    private void Load(PostEditDto post)
    {
        saved = post;
        model = new SavePostRequest
        {
            Id = post.Id,
            Title = post.Title,
            Slug = post.Slug,
            Summary = post.Summary,
            ContentFormat = post.ContentFormat,
            Content = post.Content,
            CoverImageUrl = post.CoverImageUrl,
            CoverImageAlt = post.CoverImageAlt,
            MetaTitle = post.MetaTitle,
            MetaDescription = post.MetaDescription,
            CanonicalUrl = post.CanonicalUrl,
            Status = post.Status,
            PublishedAt = post.PublishedAt,
            IsFeatured = post.IsFeatured,
            CommentsEnabled = post.CommentsEnabled,
            CategoryId = post.CategoryId,
        };
        tags = [.. post.Tags.Select(name => new TagChoice(name))];
        publishLocal = ToLocalInput(post.PublishedAt);
    }

    private async Task SaveAsync()
    {
        saving = true;
        errors.Clear();
        model.Tags = [.. tags.Select(tag => tag.Name)];

        try
        {
            var result = await Posts.SaveAsync(model);
            if (!result.IsSuccess)
            {
                errors = result.ValidationErrors
                    .Where(error => !string.IsNullOrEmpty(error.Identifier))
                    .GroupBy(error => error.Identifier, StringComparer.Ordinal)
                    .ToDictionary(group => group.Key, group => group.First().ErrorMessage, StringComparer.Ordinal);

                if (errors.Keys.Any(IsDetailsField))
                {
                    showDetails = true;
                }

                Toasts.ShowFailure(result);
                return;
            }

            bool created = IsNew;
            Load(result.Value);
            Toasts.Success(result.Value.State switch
            {
                PostState.Published => $"Published “{result.Value.Title}”.",
                PostState.Scheduled => $"Scheduled “{result.Value.Title}” for {FormatLocal(result.Value.PublishedAt!.Value)}.",
                _ => "Draft saved.",
            });

            if (created)
            {
                Navigation.NavigateTo(Urls.AdminEditPost(result.Value.Id), replace: true);
            }
        }
        finally
        {
            saving = false;
        }
    }

    private async Task DeleteAsync()
    {
        if (PostId is not int id)
        {
            return;
        }

        bool confirmed = await Dialogs.ConfirmAsync(
            "Delete post",
            $"Delete “{saved?.Title ?? model.Title}”? Its address will stop working.",
            "Delete",
            isDestructive: true);

        if (!confirmed)
        {
            return;
        }

        var deleted = await Posts.DeleteAsync(id);
        if (!deleted.IsSuccess)
        {
            Toasts.ShowFailure(deleted);
            return;
        }

        Toasts.Success("Post deleted.");
        Navigation.NavigateTo(Urls.Admin());
    }

    private void PublishDateChanged(ChangeEventArgs args)
    {
        publishLocal = args.Value?.ToString();
        model.PublishedAt = DateTime.TryParseExact(publishLocal, LocalInputFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var local)
            ? time.ToUtc(local)
            : null;
    }

    private async Task<IReadOnlyList<TagChoice>> SearchTagsAsync(string term)
    {
        var found = await Tags.SearchAsync(term);
        return found.IsSuccess ? [.. found.Value.Select(tag => new TagChoice(tag.Name))] : [];
    }

    private Task AddTagAsync(string name)
    {
        tags.Add(new TagChoice(name.Trim()));
        return Task.CompletedTask;
    }

    private string? ErrorFor(string field) => errors.GetValueOrDefault(field);

    private string? Invalid(string field) => errors.ContainsKey(field) ? "invalid" : null;

    private static bool IsDetailsField(string field) =>
        field is nameof(SavePostRequest.MetaTitle) or nameof(SavePostRequest.MetaDescription) or nameof(SavePostRequest.CanonicalUrl);

    private string? ToLocalInput(DateTime? utc) =>
        utc is DateTime value ? time.ToLocal(value).ToString(LocalInputFormat, CultureInfo.InvariantCulture) : null;

    private string FormatLocal(DateTime utc) =>
        time.ToLocal(utc).ToString("d MMM yyyy, HH:mm", CultureInfo.CurrentCulture) + (time.IsUtc ? " UTC" : string.Empty);

    /// <summary>The tag picker needs a reference type; names are all the editor holds until the post is saved.</summary>
    private sealed record TagChoice(string Name);
}