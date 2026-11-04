using Pickstack.FileManager;
using Pickstack.RichTextEditor;

namespace BlogKrafter.Infrastructure;

/// <summary>
/// Which editing tools this site offers. The host chooses its rich text editor and file manager by
/// registering them with Pickstack; BlogKrafter only asks whether they are there.
/// </summary>
internal sealed class EditorCapabilities(IServiceProvider services, IOptions<BlogKrafterOptions> options)
{
    public bool HasRichTextEditor { get; } = services.GetService<IRichTextEditor>() is not null;

    public bool HasFileManager { get; } = services.GetService<IFileManager>() is not null;

    public bool AllowsMarkdown => options.Value.EditorMode != BlogEditorMode.RichText || !HasRichTextEditor;

    public bool AllowsRichText => options.Value.EditorMode != BlogEditorMode.Markdown && HasRichTextEditor;

    public bool CanChooseFormat => AllowsMarkdown && AllowsRichText;

    /// <summary>The format a new post opens in.</summary>
    public ContentFormat DefaultFormat => (AllowsMarkdown, AllowsRichText) switch
    {
        (true, true) => options.Value.DefaultContentFormat,
        (false, true) => ContentFormat.Html,
        _ => ContentFormat.Markdown,
    };

    /// <summary>Whether an existing post in <paramref name="format"/> can be edited as it is.</summary>
    public bool Allows(ContentFormat format) => format == ContentFormat.Html ? AllowsRichText : AllowsMarkdown;
}