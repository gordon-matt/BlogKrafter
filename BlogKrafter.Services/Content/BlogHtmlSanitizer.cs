using Ganss.Xss;

namespace BlogKrafter.Content;

/// <summary>
/// Cleans post HTML before it is stored. Both editors can produce markup (Markdown allows inline HTML),
/// so nothing reaches a reader unsanitized. Register another implementation to apply a site's own policy.
/// </summary>
public interface IBlogHtmlSanitizer
{
    string Sanitize(string html);
}

public sealed class BlogHtmlSanitizer : IBlogHtmlSanitizer
{
    private readonly HtmlSanitizer sanitizer;

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

    public string Sanitize(string html) => string.IsNullOrWhiteSpace(html) ? string.Empty : sanitizer.Sanitize(html);
}