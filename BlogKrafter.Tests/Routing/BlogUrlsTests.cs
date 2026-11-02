namespace BlogKrafter.Tests.Routing;

public class BlogUrlsTests
{
    [Fact]
    public void Urls_use_the_default_paths()
    {
        var urls = new BlogUrls(Options.Create(new BlogKrafterOptions()));

        Assert.Equal("/blog", urls.Index());
        Assert.Equal("/blog/hello-world", urls.Post("hello-world"));
        Assert.Equal("/blog/hello-world?preview=true", urls.Preview("hello-world"));
        Assert.Equal("/blog/tag/dotnet?page=2", urls.Tag("dotnet", 2));
        Assert.Equal("/blog/feeds/rss.xml", urls.Rss());
        Assert.Equal("/admin/blog/posts/7", urls.AdminEditPost(7));
    }

    [Fact]
    public void Urls_follow_a_custom_base_path()
    {
        var urls = new BlogUrls(Options.Create(new BlogKrafterOptions { BasePath = "/posts", AdminPath = "/manage/posts" }));

        Assert.Equal("/posts?q=blazor&page=3", urls.Index(3, " blazor "));
        Assert.Equal("/posts/category/news", urls.Category("news"));
        Assert.Equal("/posts/archive", urls.Archive());
        Assert.Equal("/manage/posts/tags", urls.AdminTags());
    }

    [Fact]
    public void Index_is_the_site_root_when_the_blog_is_the_whole_site()
    {
        var urls = new BlogUrls(Options.Create(new BlogKrafterOptions { BasePath = string.Empty }));

        Assert.Equal("/", urls.Index());
        Assert.Equal("/first-post", urls.Post("first-post"));
    }

    [Theory]
    [InlineData("posts", "/posts")]
    [InlineData("/posts/", "/posts")]
    [InlineData(" /news/posts ", "/news/posts")]
    [InlineData("/", "")]
    [InlineData(null, "/blog")]
    [InlineData("  ", "/blog")]
    public void NormalizePath_adds_a_leading_slash_and_drops_the_trailing_one(string? path, string expected)
    {
        Assert.Equal(expected, BlogUrls.NormalizePath(path, "/blog"));
    }

    [Theory]
    [InlineData("https://example.com", "/blog/a", "https://example.com/blog/a")]
    [InlineData("https://example.com/site/", "/blog/a", "https://example.com/site/blog/a")]
    [InlineData("https://example.com", "https://cdn.example.com/a.png", "https://cdn.example.com/a.png")]
    public void Absolute_joins_the_site_address_to_a_path(string siteUrl, string path, string expected)
    {
        Assert.Equal(expected, BlogUrls.Absolute(siteUrl, path));
    }
}
