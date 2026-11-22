using Ganss.Xss;

namespace BlogKrafter.Content;

/// <summary>
/// Cleans post HTML before it is stored. Both editors can produce markup (Markdown allows inline HTML),
/// so nothing reaches a reader unsanitized. Register another implementation to apply a site's own policy.
/// </summary>
public interface IBlogHtmlSanitizer
{
    /// <summary>Returns the HTML the host's policy allows.</summary>
    /// <param name="html">The markup produced from a post.</param>
    /// <returns>The cleaned markup.</returns>
    string Sanitize(string html);
}

/// <summary>
/// The default sanitizer. It uses <see cref="Ganss.Xss.HtmlSanitizer"/> and also allows figure, figcaption, and iframe, plus the class, target, rel, loading, allow, allowfullscreen, and frameborder attributes and the mailto and tel schemes.
/// </summary>
public sealed class BlogHtmlSanitizer : IBlogHtmlSanitizer
{
    private readonly HtmlSanitizer sanitizer;

    /// <summary>Creates the sanitizer with those extra tags, attributes, and schemes.</summary>
    public BlogHtmlSanitizer()
    {
        sanitizer = new HtmlSanitizer();
        sanitizer.AllowedTags.Add("figure");
        sanitizer.AllowedTags.Add("figcaption");
        sanitizer.AllowedTags.Add("iframe");
        sanitizer.AllowedAttributes.Add("class");
        sanitizer.AllowedAttributes.Add("target");
        sanitizer.AllowedAttributes.Add("rel");
        sanitizer.AllowedAttributes.Add("loading");
        sanitizer.AllowedAttributes.Add("allow");
        sanitizer.AllowedAttributes.Add("allowfullscreen");
        sanitizer.AllowedAttributes.Add("frameborder");
        sanitizer.AllowedSchemes.Add("mailto");
        sanitizer.AllowedSchemes.Add("tel");
    }

    /// <summary>Cleans the markup. Blank or white space returns an empty string. Frames from other hosts are removed later, while the post is rendered.</summary>
    /// <param name="html">The markup produced from a post.</param>
    /// <returns>The cleaned markup, or an empty string.</returns>
    public string Sanitize(string html) => string.IsNullOrWhiteSpace(html) ? string.Empty : sanitizer.Sanitize(html);
}