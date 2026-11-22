using BlogKrafter.Content;
using BlogKrafter.Infrastructure;
using Heroicsoft.NetForge.Components.Dialogs;
using Heroicsoft.NetForge.Components.Editors;
using Heroicsoft.NetForge.Components.Toasts;
using Microsoft.JSInterop;

namespace BlogKrafter.Components.Admin;

/// <summary>
/// The body of a post in whichever editor the site offers: Markdown, or the rich text editor the host
/// registered with Pickstack. Switching converts the content so nothing is lost.
/// </summary>
public partial class ContentEditor : IDisposable
{
    private int editorGeneration;
    private MarkdownEditor? markdown;
    private string? pickError;
    private DotNetObjectReference<ContentEditor>? selfReference;

    /// <summary>
    /// Whether the body is Markdown or HTML. Changing Markdown to HTML renders it first.
    /// Changing HTML back keeps that HTML as the Markdown source.
    /// </summary>
    [Parameter]
    public ContentFormat Format { get; set; }

    /// <summary>Raised when the author switches format.</summary>
    [Parameter]
    public EventCallback<ContentFormat> FormatChanged { get; set; }

    /// <summary>The source the editor shows.</summary>
    [Parameter]
    public string Content { get; set; } = string.Empty;

    /// <summary>Raised when the source changes.</summary>
    [Parameter]
    public EventCallback<string> ContentChanged { get; set; }

    [Inject]
    private EditorCapabilities Capabilities { get; set; } = default!;

    [Inject]
    private IBlogContentRenderer Renderer { get; set; } = default!;

    [Inject]
    private IBlogFileRootProvider FileRoots { get; set; } = default!;

    [Inject]
    private DialogService Dialogs { get; set; } = default!;

    [Inject]
    private ToastService Toasts { get; set; } = default!;

    [Inject]
    private BlogScript Script { get; set; } = default!;

    /// <summary>
    /// Markdown becomes the HTML it renders to. HTML is kept as it is when going the other way:
    /// Markdown allows HTML, so the post looks the same and can be rewritten at leisure.
    /// </summary>
    private async Task SwitchAsync(ContentFormat target)
    {
        if (target == Format)
        {
            return;
        }

        bool hasContent = !string.IsNullOrWhiteSpace(Content);
        if (hasContent)
        {
            string message = target == ContentFormat.Html
                ? "The Markdown is converted to formatted text. Switching back later keeps the formatting as HTML."
                : "The formatted text is kept as HTML inside the Markdown. It displays the same, and you can replace it with Markdown as you edit.";

            if (!await Dialogs.ConfirmAsync("Change editor", message, "Change editor"))
            {
                return;
            }
        }

        string converted = hasContent && target == ContentFormat.Html
            ? Renderer.Render(Content, ContentFormat.Markdown).Html
            : Content;

        // Content first: the new editor takes its starting value when it is created, which happens when
        // the format changes, and an editor that is still mounting ignores later values.
        editorGeneration++;
        await ContentChanged.InvokeAsync(converted);
        await FormatChanged.InvokeAsync(target);

        if (hasContent)
        {
            Toasts.Info(target == ContentFormat.Html ? "Converted to rich text." : "Switched to Markdown.");
        }
    }

    private Task HtmlChangedAsync(ChangeEventArgs args) => ContentChanged.InvokeAsync(args.Value?.ToString() ?? string.Empty);

    private async Task PickImageAsync()
    {
        pickError = null;
        selfReference ??= DotNetObjectReference.Create(this);

        try
        {
            await Script.PickFileAsync(selfReference, nameof(InsertImageAsync), "image");
        }
        catch (JSException)
        {
            pickError = "The file picker did not open. Check that the site registers a Pickstack file manager.";
        }
    }

    /// <summary>Called by the file picker. A blank address is ignored; any other address is inserted as a Markdown image.</summary>
    /// <param name="url">The address the picker returned.</param>
    /// <returns>A task that completes once the image has been inserted, or immediately when there is nothing to insert.</returns>
    [JSInvokable]
    public async Task InsertImageAsync(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || markdown is null)
        {
            return;
        }

        string image = $"![Describe the image]({url.Replace(" ", "%20", StringComparison.Ordinal)})";
        await markdown.InsertTextAsync(image);
    }

    /// <inheritdoc />
    public void Dispose() => selfReference?.Dispose();
}