using BlogKrafter.Taxonomy;
using Heroicsoft.NetForge.Components.Dialogs;
using Heroicsoft.NetForge.Components.Toasts;
using Heroicsoft.NetForge.Results;

namespace BlogKrafter.Components.Admin;

/// <summary>
/// Represents the tags screen.
/// </summary>
public partial class TagsScreen
{
    private IReadOnlyList<TagDto>? tags;
    private string? loadError;

    [Inject]
    private IBlogTagService Tags { get; set; } = default!;

    [Inject]
    private IOptions<BlogKrafterOptions> Options { get; set; } = default!;

    [Inject]
    private BlogUrls Urls { get; set; } = default!;

    [Inject]
    private DialogService Dialogs { get; set; } = default!;

    [Inject]
    private ToastService Toasts { get; set; } = default!;

    /// <inheritdoc/>
    protected override Task OnInitializedAsync() => LoadAsync();

    private async Task LoadAsync()
    {
        var found = await Tags.ListAsync();
        tags = found.IsSuccess ? found.Value : null;
        loadError = found.IsSuccess ? null : found.GetDisplayMessage();
    }

    private async Task RenameAsync(TagDto tag)
    {
        string? name = await Dialogs.PromptAsync(
            "Rename tag",
            "Name",
            tag.Name,
            "Rename",
            description: "Using the name of another tag merges the two.",
            maxLength: 64);

        if (string.IsNullOrWhiteSpace(name) || string.Equals(name.Trim(), tag.Name, StringComparison.Ordinal))
        {
            return;
        }

        var renamed = await Tags.RenameAsync(tag.Id, name);
        if (!renamed.IsSuccess)
        {
            Toasts.ShowFailure(renamed);
            return;
        }

        Toasts.Success(renamed.Value.Id == tag.Id
            ? $"Renamed to “{renamed.Value.Name}”."
            : $"Merged “{tag.Name}” into “{renamed.Value.Name}”.");
        await LoadAsync();
    }

    private async Task DeleteAsync(TagDto tag)
    {
        string posts = tag.PostCount switch
        {
            0 => "No posts use it.",
            1 => "It will be removed from one post.",
            _ => $"It will be removed from {tag.PostCount} posts.",
        };

        if (!await Dialogs.ConfirmAsync("Delete tag", $"Delete “{tag.Name}”? {posts}", "Delete", isDestructive: true))
        {
            return;
        }

        var deleted = await Tags.DeleteAsync(tag.Id);
        if (!deleted.IsSuccess)
        {
            Toasts.ShowFailure(deleted);
            return;
        }

        Toasts.Success($"Deleted “{tag.Name}”.");
        await LoadAsync();
    }
}