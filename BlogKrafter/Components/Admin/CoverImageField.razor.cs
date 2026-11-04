using Microsoft.JSInterop;

namespace BlogKrafter.Components.Admin;

/// <summary>
/// A post's cover image: typed, or chosen with the file manager the host registered with Pickstack.
/// </summary>
public partial class CoverImageField : IDisposable
{
    private string? pickError;
    private DotNetObjectReference<CoverImageField>? selfReference;

    [Parameter]
    public string? Url { get; set; }

    [Parameter]
    public EventCallback<string?> UrlChanged { get; set; }

    [Parameter]
    public string? Alt { get; set; }

    [Parameter]
    public EventCallback<string?> AltChanged { get; set; }

    [Parameter]
    public string? UrlError { get; set; }

    [Parameter]
    public string? AltError { get; set; }

    [Inject]
    private EditorCapabilities Capabilities { get; set; } = default!;

    [Inject]
    private BlogScript Script { get; set; } = default!;

    private async Task BrowseAsync()
    {
        pickError = null;
        selfReference ??= DotNetObjectReference.Create(this);

        try
        {
            await Script.PickFileAsync(selfReference, nameof(ImageChosenAsync), "image");
        }
        catch (JSException)
        {
            pickError = "The file picker did not open. Check that the site registers a Pickstack file manager.";
        }
    }

    [JSInvokable]
    public async Task ImageChosenAsync(string? url)
    {
        if (!string.IsNullOrWhiteSpace(url))
        {
            await InvokeAsync(() => UrlChanged.InvokeAsync(url));
        }
    }

    private Task UrlEditedAsync(ChangeEventArgs args) => UrlChanged.InvokeAsync(args.Value?.ToString());

    private Task AltEditedAsync(ChangeEventArgs args) => AltChanged.InvokeAsync(args.Value?.ToString());

    private async Task RemoveAsync()
    {
        await UrlChanged.InvokeAsync(null);
        await AltChanged.InvokeAsync(null);
    }

    public void Dispose() => selfReference?.Dispose();
}