using System.Globalization;
using System.Text;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Markdig;

namespace BlogKrafter.Content;

/// <summary>A post body ready to store: safe HTML plus what the lists and the post page derive from it.</summary>
public sealed record RenderedContent(string Html, string Excerpt, int WordCount, IReadOnlyList<TocEntry> Headings);

/// <summary>Turns a post's source into the HTML readers get. Register another implementation to change the pipeline.</summary>
public interface IBlogContentRenderer
{
    /// <summary>
    /// Renders the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="format">The format.</param>
    /// <returns>The rendered content.</returns>
    RenderedContent Render(string? source, ContentFormat format);

    /// <summary>Reads the table of contents back out of HTML this renderer produced.</summary>
    IReadOnlyList<TocEntry> ReadHeadings(string html);
}

/// <summary>
/// Represents the blog content renderer.
/// </summary>
public sealed class BlogContentRenderer(IBlogHtmlSanitizer sanitizer, IOptions<BlogKrafterOptions> options) : IBlogContentRenderer
{
    private const int ExcerptLength = 280;

    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    /// <summary>
    /// Renders the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="format">The format.</param>
    /// <returns>The rendered content.</returns>
    public RenderedContent Render(string? source, ContentFormat format)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return new RenderedContent(string.Empty, string.Empty, 0, []);
        }

        string html = format == ContentFormat.Markdown ? Markdown.ToHtml(source, Pipeline) : source;
        string clean = sanitizer.Sanitize(html);

        var body = Parse(clean);
        var headings = AssignHeadingIds(body);
        RemoveDisallowedFrames(body);
        TidyLinksAndImages(body);

        string text = NormalizeWhitespace(body.TextContent);
        int words = text.Length == 0 ? 0 : text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

        return new RenderedContent(body.InnerHtml, CreateExcerpt(body), words, headings);
    }

    /// <summary>
    /// Reads the headings.
    /// </summary>
    /// <param name="html">The HTML.</param>
    /// <returns>The headings.</returns>
    public IReadOnlyList<TocEntry> ReadHeadings(string html) => string.IsNullOrWhiteSpace(html)
            ? []
            : [.. Parse(html).QuerySelectorAll("h2[id], h3[id]")
            .Select(heading => new TocEntry(heading.Id!, NormalizeWhitespace(heading.TextContent), heading.LocalName == "h2" ? 2 : 3))
            .Where(entry => entry.Text.Length > 0)];

    /// <summary>Shortens plain text to about <paramref name="length"/> characters, ending on a word.</summary>
    public static string Truncate(string text, int length = ExcerptLength)
    {
        string normalized = NormalizeWhitespace(text);
        if (normalized.Length <= length)
        {
            return normalized;
        }

        int cut = normalized.LastIndexOf(' ', length);
        return normalized[..(cut > length / 2 ? cut : length)].TrimEnd(',', ';', ':', '.', ' ') + "…";
    }

    private static IElement Parse(string html)
    {
        var document = new HtmlParser().ParseDocument(string.Empty);
        var body = document.Body!;
        body.InnerHtml = html;
        return body;
    }

    /// <summary>
    /// Gives second- and third-level headings stable, unique anchors so the table of contents and shared
    /// links can point into the post. Ids from the source are replaced, which also stops pasted HTML from
    /// claiming ids the page itself uses.
    /// </summary>
    private static List<TocEntry> AssignHeadingIds(IElement body)
    {
        var used = new HashSet<string>(StringComparer.Ordinal);
        var entries = new List<TocEntry>();

        foreach (var heading in body.QuerySelectorAll("h2, h3"))
        {
            string text = NormalizeWhitespace(heading.TextContent);
            string id = Slugs.Generate(text, 80);
            if (id.Length == 0)
            {
                id = "section";
            }

            string unique = id;
            for (int n = 2; !used.Add(unique); n++)
            {
                unique = id + "-" + n.ToString(CultureInfo.InvariantCulture);
            }

            heading.Id = unique;
            if (text.Length > 0)
            {
                entries.Add(new TocEntry(unique, text, heading.LocalName == "h2" ? 2 : 3));
            }
        }

        return entries;
    }

    private void RemoveDisallowedFrames(IElement body)
    {
        var allowedHosts = options.Value.AllowedFrameHosts;

        foreach (var frame in body.QuerySelectorAll("iframe").ToList())
        {
            bool allowed = Uri.TryCreate(frame.GetAttribute("src"), UriKind.Absolute, out var source)
                && source.Scheme == Uri.UriSchemeHttps
                && allowedHosts.Contains(source.Host, StringComparer.OrdinalIgnoreCase);

            if (!allowed)
            {
                frame.Remove();
                continue;
            }

            frame.SetAttribute("loading", "lazy");
        }
    }

    private static void TidyLinksAndImages(IElement body)
    {
        foreach (var image in body.QuerySelectorAll("img"))
        {
            if (!image.HasAttribute("loading"))
            {
                image.SetAttribute("loading", "lazy");
            }
        }

        foreach (var link in body.QuerySelectorAll("a[target]"))
        {
            if (string.Equals(link.GetAttribute("target"), "_blank", StringComparison.OrdinalIgnoreCase))
            {
                link.SetAttribute("rel", "noopener noreferrer");
            }
        }
    }

    /// <summary>
    /// Built from paragraphs, so a post that opens with a code sample or a table does not show that as
    /// its summary. Falls back to all the text when there are no paragraphs.
    /// </summary>
    private static string CreateExcerpt(IElement body)
    {
        var builder = new StringBuilder();
        foreach (var paragraph in body.QuerySelectorAll("p"))
        {
            if (paragraph.Closest("blockquote, pre, table, figure") is not null)
            {
                continue;
            }

            builder.Append(paragraph.TextContent).Append(' ');
            if (builder.Length > ExcerptLength * 2)
            {
                break;
            }
        }

        string text = builder.Length > 0 ? builder.ToString() : body.TextContent;
        return Truncate(text);
    }

    private static string NormalizeWhitespace(string text)
    {
        var builder = new StringBuilder(text.Length);
        bool space = false;
        foreach (char c in text)
        {
            if (char.IsWhiteSpace(c))
            {
                space = builder.Length > 0;
                continue;
            }

            if (space)
            {
                builder.Append(' ');
                space = false;
            }

            builder.Append(c);
        }

        return builder.ToString();
    }
}