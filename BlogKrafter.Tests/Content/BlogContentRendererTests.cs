namespace BlogKrafter.Tests.Content;

public class BlogContentRendererTests
{
    private static BlogContentRenderer CreateRenderer(BlogKrafterOptions? options = null) =>
        new(new BlogHtmlSanitizer(), Options.Create(options ?? new BlogKrafterOptions()));

    [Fact]
    public void Render_turns_markdown_into_html()
    {
        var rendered = CreateRenderer().Render("Some **bold** text.", ContentFormat.Markdown);

        Assert.Contains("<strong>bold</strong>", rendered.Html);
        Assert.Equal("Some bold text.", rendered.Excerpt);
        Assert.Equal(3, rendered.WordCount);
    }

    [Fact]
    public void Render_strips_scripts_and_event_handlers()
    {
        const string html = "<p onclick=\"steal()\">Hi</p><script>alert(1)</script><a href=\"javascript:alert(1)\">x</a>";

        var rendered = CreateRenderer().Render(html, ContentFormat.Html);

        Assert.DoesNotContain("script", rendered.Html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onclick", rendered.Html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("javascript:", rendered.Html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Render_strips_script_written_inline_in_markdown()
    {
        var rendered = CreateRenderer().Render("Text\n\n<script>alert(1)</script>", ContentFormat.Markdown);

        Assert.DoesNotContain("<script", rendered.Html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Render_gives_headings_unique_anchors_and_lists_them()
    {
        const string markdown = "## Getting started\n\ntext\n\n### Install\n\ntext\n\n## Getting started\n\ntext";

        var rendered = CreateRenderer().Render(markdown, ContentFormat.Markdown);

        Assert.Equal(
            [new TocEntry("getting-started", "Getting started", 2), new TocEntry("install", "Install", 3), new TocEntry("getting-started-2", "Getting started", 2)],
            rendered.Headings);
        Assert.Contains("id=\"getting-started-2\"", rendered.Html);
    }

    [Fact]
    public void Render_replaces_heading_ids_from_the_source()
    {
        var rendered = CreateRenderer().Render("<h2 id=\"main\">Intro</h2>", ContentFormat.Html);

        Assert.Contains("id=\"intro\"", rendered.Html);
        Assert.DoesNotContain("id=\"main\"", rendered.Html);
    }

    [Fact]
    public void ReadHeadings_reads_back_the_anchors_render_wrote()
    {
        var renderer = CreateRenderer();
        var rendered = renderer.Render("## One\n\n## Two", ContentFormat.Markdown);

        Assert.Equal(rendered.Headings, renderer.ReadHeadings(rendered.Html));
    }

    [Fact]
    public void Render_keeps_frames_only_from_allowed_https_hosts()
    {
        const string html =
            "<iframe src=\"https://www.youtube.com/embed/abc\"></iframe>" +
            "<iframe src=\"https://evil.example/embed\"></iframe>" +
            "<iframe src=\"http://player.vimeo.com/video/1\"></iframe>";

        var rendered = CreateRenderer().Render(html, ContentFormat.Html);

        Assert.Contains("www.youtube.com/embed/abc", rendered.Html);
        Assert.Contains("loading=\"lazy\"", rendered.Html);
        Assert.DoesNotContain("evil.example", rendered.Html);
        Assert.DoesNotContain("vimeo", rendered.Html);
    }

    [Fact]
    public void Render_protects_links_that_open_a_new_window()
    {
        var rendered = CreateRenderer().Render("<a href=\"https://example.com\" target=\"_blank\">x</a>", ContentFormat.Html);

        Assert.Contains("rel=\"noopener noreferrer\"", rendered.Html);
    }

    [Fact]
    public void Render_builds_the_excerpt_from_paragraphs_not_code()
    {
        const string markdown = "```\nvar x = 1;\n```\n\nThe real opening line.";

        var rendered = CreateRenderer().Render(markdown, ContentFormat.Markdown);

        Assert.Equal("The real opening line.", rendered.Excerpt);
    }

    [Fact]
    public void Truncate_ends_on_a_word()
    {
        string text = string.Join(' ', Enumerable.Repeat("word", 100));

        string truncated = BlogContentRenderer.Truncate(text, 22);

        Assert.Equal("word word word word…", truncated);
    }
}