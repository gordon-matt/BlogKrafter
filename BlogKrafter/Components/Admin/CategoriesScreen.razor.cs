using BlogKrafter.Taxonomy;
using Heroicsoft.NetForge.Components.Dialogs;
using Heroicsoft.NetForge.Components.Toasts;
using Heroicsoft.NetForge.Results;

namespace BlogKrafter.Components.Admin;

/// <summary>
/// Represents the categories screen.
/// </summary>
public partial class CategoriesScreen
{
    private IReadOnlyList<CategoryDto>? categories;
    private string? loadError;
    private SaveCategoryRequest form = new();
    private Dictionary<string, string> errors = new(StringComparer.Ordinal);
    private bool editing;
    private bool saving;

    [Inject]
    private IBlogCategoryService Categories { get; set; } = default!;

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
        var found = await Categories.ListAsync();
        categories = found.IsSuccess ? found.Value : null;
        loadError = found.IsSuccess ? null : found.GetDisplayMessage();
    }

    private void Edit(CategoryDto? category)
    {
        errors.Clear();
        form = category is null
            ? new SaveCategoryRequest { SortOrder = (categories?.Count ?? 0) * 10 }
            : new SaveCategoryRequest
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description,
                SortOrder = category.SortOrder,
            };
        editing = true;
    }

    private void Close() => editing = false;

    private async Task SaveAsync()
    {
        saving = true;
        try
        {
            var result = await Categories.SaveAsync(form);
            if (!result.IsSuccess)
            {
                errors = result.ValidationErrors
                    .Where(error => !string.IsNullOrEmpty(error.Identifier))
                    .GroupBy(error => error.Identifier, StringComparer.Ordinal)
                    .ToDictionary(group => group.Key, group => group.First().ErrorMessage, StringComparer.Ordinal);

                if (errors.Count == 0)
                {
                    Toasts.ShowFailure(result);
                }

                return;
            }

            Toasts.Success(form.Id is null ? $"Added “{result.Value.Name}”." : $"Saved “{result.Value.Name}”.");
            editing = false;
            await LoadAsync();
        }
        finally
        {
            saving = false;
        }
    }

    private async Task DeleteAsync(CategoryDto category)
    {
        string posts = category.PostCount switch
        {
            0 => "No posts use it.",
            1 => "Its one post will have no category.",
            _ => $"Its {category.PostCount} posts will have no category.",
        };

        if (!await Dialogs.ConfirmAsync("Delete category", $"Delete “{category.Name}”? {posts}", "Delete", isDestructive: true))
        {
            return;
        }

        var deleted = await Categories.DeleteAsync(category.Id);
        if (!deleted.IsSuccess)
        {
            Toasts.ShowFailure(deleted);
            return;
        }

        Toasts.Success($"Deleted “{category.Name}”.");
        await LoadAsync();
    }

    private string? ErrorFor(string field) => errors.GetValueOrDefault(field);
}