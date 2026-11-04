using System.Xml.Linq;
using BlogKrafter.Feeds;

namespace BlogKrafter.Tests.Feeds;

public class BlogFeedWriterTests
{
    private static readonly XNamespace Atom = "http://www.w3.org/2005/Atom";
    private static readonly XNamespace Content = "http://purl.org/rss/1.0/modules/content/";
    private static readonly XNamespace Sitemap = "http://www.sitemaps.org/schemas/sitemap/0.9";

    private static FeedItem Item(string slug, string html = "<p>Body</p>") => new(
        new PostSummaryDto(
            1,
            "A <title> & more",
            slug,
            "The excerpt",
            null,
            null,
            new DateTime(2026, 5, 1, 9, 30, 0, DateTimeKind.Utc),
            new DateTime(2026, 5, 2, 10, 0, 0, DateTimeKind.Utc),
            2,
            "Ada Editor",
            false,
            new CategoryRef(1, "News", "news"),
            [new TagRef(1, "Blazor", "blazor")]),
        html);

    [Fact]
    public void WriteRss_writes_absolute_links_and_the_full_body()
    {
        using var db = new TestDatabase();
        db.Options.Title = "Field Notes";

        string xml = db.CreateFeedWriter().WriteRss([Item("first-post", "<p><img src=\"/media/a.png\"></p>")], "https://example.com");

        var channel = XDocument.Parse(xml).Root!.Element("channel")!;
        var item = channel.Element("item")!;
        Assert.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>", xml);
        Assert.Equal("Field Notes", channel.Element("title")!.Value);
        Assert.Equal("https://example.com/blog/feeds/rss.xml", channel.Element(Atom + "link")!.Attribute("href")!.Value);
        Assert.Equal("A <title> & more", item.Element("title")!.Value);
        Assert.Equal("https://example.com/blog/first-post", item.Element("link")!.Value);
        Assert.Equal("Fri, 01 May 2026 09:30:00 GMT", item.Element("pubDate")!.Value);
        Assert.Contains("https://example.com/media/a.png", item.Element(Content + "encoded")!.Value);
    }

    [Fact]
    public void WriteAtom_writes_one_entry_per_post()
    {
        using var db = new TestDatabase();

        string xml = db.CreateFeedWriter().WriteAtom([Item("one"), Item("two")], "https://example.com");

        var entries = XDocument.Parse(xml).Root!.Elements(Atom + "entry").ToList();
        Assert.Equal(2, entries.Count);
        Assert.Equal("2026-05-02T10:00:00Z", entries[0].Element(Atom + "updated")!.Value);
    }

    [Fact]
    public void WriteSitemap_follows_a_custom_base_path()
    {
        using var db = new TestDatabase();
        db.Options.BasePath = "/posts";

        string xml = db.CreateFeedWriter().WriteSitemap(
            [new SitemapEntry("/posts/one", new DateTime(2026, 5, 2, 0, 0, 0, DateTimeKind.Utc))],
            "https://example.com");

        var locations = XDocument.Parse(xml).Descendants(Sitemap + "loc").Select(element => element.Value).ToList();
        Assert.Contains("https://example.com/posts/one", locations);
    }

    [Fact]
    public void AbsolutizeLinks_rewrites_only_root_relative_addresses()
    {
        const string html = "<a href=\"/blog/other\">a</a><a href=\"//cdn.example.com/x\">b</a><img src=\"https://elsewhere.example/c.png\">";

        string result = BlogFeedWriter.AbsolutizeLinks(html, "https://example.com");

        Assert.Contains("href=\"https://example.com/blog/other\"", result);
        Assert.Contains("href=\"//cdn.example.com/x\"", result);
        Assert.Contains("src=\"https://elsewhere.example/c.png\"", result);
    }
}