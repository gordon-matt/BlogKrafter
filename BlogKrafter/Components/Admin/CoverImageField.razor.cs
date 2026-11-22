using Microsoft.JSInterop;

namespace BlogKrafter.Components.Admin;

/// <summary>
/// A post's cover image: typed, or chosen with the file manager the host registered with Pickstack.
/// </summary>
public partial class CoverImageField : IDisposable
{
    private string? pickError;
    private DotNetObjectReference<CoverImageField>? selfReference;

    /// <summary>The cover address. Blank means no cover.</summary>
    [Parameter]
    public string? Url { get; set; }

    /// <summary>Raised when the address changes, including when the author clears it.</summary>
    [Parameter]
    public EventCallback<string?> UrlChanged { get; set; }

    /// <summary>The cover's alternative text.</summary>
    [Parameter]
    public string? Alt { get; set; }

    /// <summary>Raised when the alternative text changes.</summary>
    [Parameter]
    public EventCallback<string?> AltChanged { get; set; }

    /// <summary>A validation message for the address. <see langword="null"/> shows none.</summary>
    [Parameter]
    public string? UrlError { get; set; }

    /// <summary>A validation message for the alternative text. <see langword="null"/> shows none.</summary>
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

    /// <summary>Called by the file picker. A blank address is ignored; any other address is written through <see cref="UrlChanged"/>.</summary>
    /// <param name="url">The address the picker returned.</param>
    /// <returns>A task that completes once the address has been written.</returns>
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

    /// <summary>Releases the reference the file picker calls back into.</summary>
    public void Dispose() => selfReference?.Dispose();
}