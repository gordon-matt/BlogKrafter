using Microsoft.JSInterop;

namespace BlogKrafter.Infrastructure;

/// <summary>The editor's browser helpers in <c>blogkrafter.js</c>, imported once per circuit.</summary>
internal sealed class BlogScript(IJSRuntime js, IBlogFileRootProvider fileRoots) : IAsyncDisposable
{
    private const string ModulePath = "./_content/BlogKrafter/blogkrafter.js";

    private Task<IJSObjectReference>? module;

    public async Task PickFileAsync<T>(DotNetObjectReference<T> receiver, string method, string type)
        where T : class =>
        await (await ModuleAsync()).InvokeVoidAsync("pickFile", receiver, method, type, fileRoots.GetFileRoot() ?? string.Empty);

    public async Task<string> WrapSelectionAsync(ElementReference textarea, string before, string after, string placeholder) =>
        await (await ModuleAsync()).InvokeAsync<string>("wrapSelection", textarea, before, after, placeholder);

    public async Task<string> PrefixLinesAsync(ElementReference textarea, string prefix) =>
        await (await ModuleAsync()).InvokeAsync<string>("prefixLines", textarea, prefix);

    public async Task<string> InsertTextAsync(ElementReference textarea, string text) =>
        await (await ModuleAsync()).InvokeAsync<string>("insertText", textarea, text);

    public async Task<string?> TimeZoneAsync() =>
        await (await ModuleAsync()).InvokeAsync<string?>("timeZone");

    public async ValueTask DisposeAsync()
    {
        if (module is { IsCompletedSuccessfully: true })
        {
            try
            {
                await module.Result.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
            }
        }
    }

    private Task<IJSObjectReference> ModuleAsync() =>
        module ??= js.InvokeAsync<IJSObjectReference>("import", ModulePath).AsTask();
}
