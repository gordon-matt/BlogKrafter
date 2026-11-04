using System.Globalization;
using BlogKrafter.Posts;
using Heroicsoft.NetForge.Components.Dialogs;
using Heroicsoft.NetForge.Components.Toasts;
using Heroicsoft.NetForge.Results;

namespace BlogKrafter.Components.Admin;

public partial class PostsScreen : IDisposable
{
    private static readonly IReadOnlyList<(PostFilter Filter, string Label)> Filters =
    [
        (PostFilter.All, "All"),
        (PostFilter.Drafts, "Drafts"),
        (PostFilter.Scheduled, "Scheduled"),
        (PostFilter.Published, "Published"),
    ];

    private static readonly IReadOnlyList<int> PageSizes = [20, 50, 100];

    private PagedList<AdminPostRow>? listing;
    private string? loadError;
    private PostFilter filter = PostFilter.All;
    private string? search;
    private int pageNumber = 1;
    private int pageSize = 20;
    private CancellationTokenSource? searchDelay;
    private BrowserTime time = BrowserTime.Utc;

    [Inject]
    private IBlogPostService Posts { get; set; } = default!;

    [Inject]
    private BlogScript Script { get; set; } = default!;

    [Inject]
    private IOptions<BlogKrafterOptions> Options { get; set; } = default!;

    [Inject]
    private BlogUrls Urls { get; set; } = default!;

    [Inject]
    private DialogService Dialogs { get; set; } = default!;

    [Inject]
    private ToastService Toasts { get; set; } = default!;

    protected override Task OnInitializedAsync() => LoadAsync();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        time = BrowserTime.From(await Script.TimeZoneAsync());
        StateHasChanged();
    }

    private async Task LoadAsync()
    {
        var found = await Posts.ListAsync(new AdminPostQuery
        {
            Page = pageNumber,
            PageSize = pageSize,
            Filter = filter,
            Search = search,
        });

        listing = found.IsSuccess ? found.Value : null;
        loadError = found.IsSuccess ? null : found.GetDisplayMessage();
    }

    private async Task SetFilterAsync(PostFilter next)
    {
        filter = next;
        pageNumber = 1;
        await LoadAsync();
    }

    private async Task SearchChangedAsync(string? value)
    {
        search = value;
        searchDelay?.Cancel();
        searchDelay?.Dispose();
        searchDelay = new CancellationTokenSource();

        try
        {
            await Task.Delay(250, searchDelay.Token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        pageNumber = 1;
        await LoadAsync();
    }

    private async Task GoToPageAsync(int next)
    {
        pageNumber = next;
        await LoadAsync();
    }

    private async Task SetPageSizeAsync(int size)
    {
        pageSize = size;
        pageNumber = 1;
        await LoadAsync();
    }

    private async Task DeleteAsync(AdminPostRow row)
    {
        bool confirmed = await Dialogs.ConfirmAsync(
            "Delete post",
            $"Delete “{row.Title}”? Its address will stop working.",
            "Delete",
            isDestructive: true);

        if (!confirmed)
        {
            return;
        }

        var deleted = await Posts.DeleteAsync(row.Id);
        if (!deleted.IsSuccess)
        {
            Toasts.ShowFailure(deleted);
            return;
        }

        Toasts.Success($"Deleted “{row.Title}”.");
        await LoadAsync();
    }

    private string ViewHref(AdminPostRow row) =>
        row.State == PostState.Published ? Urls.Post(row.Slug) : Urls.Preview(row.Slug);

    private string FormatDate(DateTime? utc) =>
        utc is DateTime value ? time.ToLocal(value).ToString("d MMM yyyy", CultureInfo.CurrentCulture) : "—";

    public void Dispose()
    {
        searchDelay?.Cancel();
        searchDelay?.Dispose();
    }
}