using BlogKrafter.Content;
using Microsoft.JSInterop;

namespace BlogKrafter.Components.Admin;

/// <summary>A Markdown text area with a formatting toolbar and a preview rendered exactly as readers will see it.</summary>
public partial class MarkdownEditor : IDisposable
{
    private ElementReference textarea;
    private bool previewing;
    private string previewHtml = string.Empty;
    private string? pickError;
    private DotNetObjectReference<MarkdownEditor>? selfReference;

    [Parameter]
    public string Value { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    [Inject]
    private IBlogContentRenderer Renderer { get; set; } = default!;

    [Inject]
    private EditorCapabilities Capabilities { get; set; } = default!;

    [Inject]
    private IOptions<BlogKrafterOptions> Options { get; set; } = default!;

    [Inject]
    private BlogScript Script { get; set; } = default!;

    private Task ChangedAsync(ChangeEventArgs args) => SetValueAsync(args.Value?.ToString() ?? string.Empty);

    private async Task SetValueAsync(string value)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);
    }

    private void ShowPreview()
    {
        previewHtml = Renderer.Render(Value, ContentFormat.Markdown).Html;
        previewing = true;
    }

    private async Task WrapAsync(string before, string after, string placeholder) =>
        await SetValueAsync(await Script.WrapSelectionAsync(textarea, before, after, placeholder));

    private async Task PrefixAsync(string prefix) =>
        await SetValueAsync(await Script.PrefixLinesAsync(textarea, prefix));

    private async Task PickImageAsync()
    {
        pickError = null;
        selfReference ??= DotNetObjectReference.Create(this);

        try
        {
            await Script.PickFileAsync(selfReference, nameof(InsertImageAsync), "image", Options.Value.FileRoot);
        }
        catch (JSException)
        {
            pickError = "The file picker did not open. Check that the site registers a Pickstack file manager.";
        }
    }

    [JSInvokable]
    public async Task InsertImageAsync(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        string markdown = $"![Describe the image]({url.Replace(" ", "%20", StringComparison.Ordinal)})";
        string value = await Script.InsertTextAsync(textarea, markdown);
        await InvokeAsync(() => SetValueAsync(value));
    }

    public void Dispose() => selfReference?.Dispose();
}
