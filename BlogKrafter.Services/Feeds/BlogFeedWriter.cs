using System.Globalization;
using System.Text;
using System.Xml;
using AngleSharp.Html.Parser;

namespace BlogKrafter.Feeds;

/// <summary>Writes the RSS, Atom and sitemap documents. Feed readers need absolute links, hence <c>siteUrl</c>.</summary>
public interface IBlogFeedWriter
{
    /// <param name="siteUrl">Scheme, host and any path base, such as <c>https://example.com</c>.</param>
    string WriteRss(IReadOnlyList<FeedItem> items, string siteUrl);

    /// <param name="siteUrl">Scheme, host and any path base, such as <c>https://example.com</c>.</param>
    string WriteAtom(IReadOnlyList<FeedItem> items, string siteUrl);

    /// <param name="siteUrl">Scheme, host and any path base, such as <c>https://example.com</c>.</param>
    string WriteSitemap(IReadOnlyList<SitemapEntry> entries, string siteUrl);
}

public sealed class BlogFeedWriter(IOptions<BlogKrafterOptions> options, BlogUrls urls, TimeProvider time) : IBlogFeedWriter
{
    private const string AtomNamespace = "http://www.w3.org/2005/Atom";
    private const string ContentNamespace = "http://purl.org/rss/1.0/modules/content/";
    private const string SitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";

    public string WriteRss(IReadOnlyList<FeedItem> items, string siteUrl)
    {
        var blog = options.Value;
        return Write(writer =>
        {
            writer.WriteStartElement("rss");
            writer.WriteAttributeString("version", "2.0");
            writer.WriteAttributeString("xmlns", "atom", null, AtomNamespace);
            writer.WriteAttributeString("xmlns", "content", null, ContentNamespace);

            writer.WriteStartElement("channel");
            writer.WriteElementString("title", blog.Title);
            writer.WriteElementString("link", BlogUrls.Absolute(siteUrl, urls.Index()));
            writer.WriteElementString("description", blog.Description ?? blog.Title);
            writer.WriteElementString("lastBuildDate", Rfc822(LastUpdated(items)));

            writer.WriteStartElement("atom", "link", AtomNamespace);
            writer.WriteAttributeString("href", BlogUrls.Absolute(siteUrl, urls.Rss()));
            writer.WriteAttributeString("rel", "self");
            writer.WriteAttributeString("type", "application/rss+xml");
            writer.WriteEndElement();

            foreach (var item in items)
            {
                var post = item.Post;
                string link = BlogUrls.Absolute(siteUrl, urls.Post(post.Slug));

                writer.WriteStartElement("item");
                writer.WriteElementString("title", post.Title);
                writer.WriteElementString("link", link);

                writer.WriteStartElement("guid");
                writer.WriteAttributeString("isPermaLink", "true");
                writer.WriteString(link);
                writer.WriteEndElement();

                writer.WriteElementString("pubDate", Rfc822(post.PublishedAt ?? post.UpdatedAt));
                writer.WriteElementString("description", post.Excerpt);

                if (post.Category is not null)
                {
                    writer.WriteElementString("category", post.Category.Name);
                }

                foreach (var tag in post.Tags)
                {
                    writer.WriteElementString("category", tag.Name);
                }

                writer.WriteStartElement("content", "encoded", ContentNamespace);
                writer.WriteCData(AbsolutizeLinks(item.ContentHtml, siteUrl));
                writer.WriteEndElement();

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
        });
    }

    public string WriteAtom(IReadOnlyList<FeedItem> items, string siteUrl)
    {
        var blog = options.Value;
        return Write(writer =>
        {
            writer.WriteStartElement("feed", AtomNamespace);
            writer.WriteElementString("title", AtomNamespace, blog.Title);
            if (!string.IsNullOrWhiteSpace(blog.Description))
            {
                writer.WriteElementString("subtitle", AtomNamespace, blog.Description);
            }

            writer.WriteElementString("id", AtomNamespace, BlogUrls.Absolute(siteUrl, urls.Index()));
            writer.WriteElementString("updated", AtomNamespace, Iso8601(LastUpdated(items)));
            WriteAtomLink(writer, BlogUrls.Absolute(siteUrl, urls.Index()), "alternate", "text/html");
            WriteAtomLink(writer, BlogUrls.Absolute(siteUrl, urls.Atom()), "self", "application/atom+xml");

            foreach (var item in items)
            {
                var post = item.Post;
                string link = BlogUrls.Absolute(siteUrl, urls.Post(post.Slug));

                writer.WriteStartElement("entry", AtomNamespace);
                writer.WriteElementString("title", AtomNamespace, post.Title);
                writer.WriteElementString("id", AtomNamespace, link);
                WriteAtomLink(writer, link, "alternate", "text/html");
                writer.WriteElementString("published", AtomNamespace, Iso8601(post.PublishedAt ?? post.UpdatedAt));
                writer.WriteElementString("updated", AtomNamespace, Iso8601(post.UpdatedAt));

                writer.WriteStartElement("author", AtomNamespace);
                writer.WriteElementString("name", AtomNamespace, post.AuthorName);
                writer.WriteEndElement();

                foreach (string term in post.Tags.Select(tag => tag.Name).Prepend(post.Category?.Name).OfType<string>())
                {
                    writer.WriteStartElement("category", AtomNamespace);
                    writer.WriteAttributeString("term", term);
                    writer.WriteEndElement();
                }

                writer.WriteElementString("summary", AtomNamespace, post.Excerpt);

                writer.WriteStartElement("content", AtomNamespace);
                writer.WriteAttributeString("type", "html");
                writer.WriteString(AbsolutizeLinks(item.ContentHtml, siteUrl));
                writer.WriteEndElement();

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        });
    }

    public string WriteSitemap(IReadOnlyList<SitemapEntry> entries, string siteUrl) => Write(writer =>
    {
        writer.WriteStartElement("urlset", SitemapNamespace);
        foreach (var entry in entries)
        {
            writer.WriteStartElement("url", SitemapNamespace);
            writer.WriteElementString("loc", SitemapNamespace, BlogUrls.Absolute(siteUrl, entry.Path));
            if (entry.LastModified is DateTime modified)
            {
                writer.WriteElementString("lastmod", SitemapNamespace, Iso8601(modified));
            }

            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    });

    /// <summary>
    /// Post bodies link to uploads and other posts with root-relative paths, which a feed reader would
    /// resolve against its own address.
    /// </summary>
    internal static string AbsolutizeLinks(string html, string siteUrl)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var body = new HtmlParser().ParseDocument(string.Empty).Body!;
        body.InnerHtml = html;

        foreach (var element in body.QuerySelectorAll("[src], [href]"))
        {
            foreach (string attribute in (string[])["src", "href"])
            {
                string? value = element.GetAttribute(attribute);
                if (value is not null && value.StartsWith('/') && !value.StartsWith("//", StringComparison.Ordinal))
                {
                    element.SetAttribute(attribute, BlogUrls.Absolute(siteUrl, value));
                }
            }
        }

        return body.InnerHtml;
    }

    private DateTime LastUpdated(IReadOnlyList<FeedItem> items) =>
        items.Count == 0 ? time.GetUtcNow().UtcDateTime : items.Max(item => item.Post.UpdatedAt);

    private static void WriteAtomLink(XmlWriter writer, string href, string rel, string type)
    {
        writer.WriteStartElement("link", AtomNamespace);
        writer.WriteAttributeString("href", href);
        writer.WriteAttributeString("rel", rel);
        writer.WriteAttributeString("type", type);
        writer.WriteEndElement();
    }

    private static string Write(Action<XmlWriter> body)
    {
        var builder = new StringBuilder();
        using (var stringWriter = new Utf8StringWriter(builder))
        using (var writer = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true }))
        {
            writer.WriteStartDocument();
            body(writer);
            writer.WriteEndDocument();
        }

        return builder.ToString();
    }

    private static string Rfc822(DateTime utc) =>
        DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToString("r", CultureInfo.InvariantCulture);

    private static string Iso8601(DateTime utc) =>
        DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);

    /// <summary>Makes the XML declaration say UTF-8, which is what the endpoint sends, rather than UTF-16.</summary>
    private sealed class Utf8StringWriter(StringBuilder builder) : StringWriter(builder, CultureInfo.InvariantCulture)
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
