using Ganss.Xss;

namespace BlogKrafter.Content;

/// <summary>
/// Cleans post HTML before it is stored. Both editors can produce markup (Markdown allows inline HTML),
/// so nothing reaches a reader unsanitized. Register another implementation to apply a site's own policy.
/// </summary>
public interface IBlogHtmlSanitizer
{
    /// <summary>
    /// Sanitizes the value.
    /// </summary>
    /// <param name="html">The HTML.</param>
    /// <returns>The string.</returns>
    string Sanitize(string html);
}

/// <summary>
/// Represents the blog HTML sanitizer.
/// </summary>
public sealed class BlogHtmlSanitizer : IBlogHtmlSanitizer
{
    private readonly HtmlSanitizer sanitizer;

    /// <summary>
    /// Initializes a new instance of the BlogHtmlSanitizer class.
    /// </summary>
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

    /// <summary>
    /// Sanitizes the value.
    /// </summary>
    /// <param name="html">The HTML.</param>
    /// <returns>The string.</returns>
    public string Sanitize(string html) => string.IsNullOrWhiteSpace(html) ? string.Empty : sanitizer.Sanitize(html);
}