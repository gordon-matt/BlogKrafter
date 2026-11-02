using BlogKrafter.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pickstack.RichTextEditor;

namespace BlogKrafter.Tests.Infrastructure;

public class EditorCapabilitiesTests
{
    private static EditorCapabilities Create(BlogEditorMode mode, bool hasRichTextEditor, ContentFormat defaultFormat = ContentFormat.Markdown)
    {
        var services = new ServiceCollection();
        if (hasRichTextEditor)
        {
            services.AddSingleton(Mock.Of<IRichTextEditor>());
        }

        return new EditorCapabilities(
            services.BuildServiceProvider(),
            Options.Create(new BlogKrafterOptions { EditorMode = mode, DefaultContentFormat = defaultFormat }));
    }

    [Fact]
    public void Both_with_an_editor_lets_authors_choose()
    {
        var capabilities = Create(BlogEditorMode.Both, hasRichTextEditor: true, ContentFormat.Html);

        Assert.True(capabilities.CanChooseFormat);
        Assert.Equal(ContentFormat.Html, capabilities.DefaultFormat);
    }

    [Fact]
    public void Both_without_an_editor_falls_back_to_markdown()
    {
        var capabilities = Create(BlogEditorMode.Both, hasRichTextEditor: false, ContentFormat.Html);

        Assert.False(capabilities.CanChooseFormat);
        Assert.False(capabilities.AllowsRichText);
        Assert.Equal(ContentFormat.Markdown, capabilities.DefaultFormat);
    }

    [Fact]
    public void RichText_without_an_editor_still_allows_markdown()
    {
        var capabilities = Create(BlogEditorMode.RichText, hasRichTextEditor: false);

        Assert.True(capabilities.Allows(ContentFormat.Markdown));
        Assert.Equal(ContentFormat.Markdown, capabilities.DefaultFormat);
    }

    [Fact]
    public void RichText_with_an_editor_hides_markdown()
    {
        var capabilities = Create(BlogEditorMode.RichText, hasRichTextEditor: true);

        Assert.False(capabilities.Allows(ContentFormat.Markdown));
        Assert.Equal(ContentFormat.Html, capabilities.DefaultFormat);
    }

    [Fact]
    public void Markdown_ignores_a_registered_editor()
    {
        var capabilities = Create(BlogEditorMode.Markdown, hasRichTextEditor: true);

        Assert.False(capabilities.Allows(ContentFormat.Html));
        Assert.Equal(ContentFormat.Markdown, capabilities.DefaultFormat);
    }
}

public class BrowserTimeTests
{
    [Fact]
    public void From_falls_back_to_utc_for_an_unknown_zone()
    {
        var time = BrowserTime.From("Not/AZone");

        Assert.True(time.IsUtc);
        Assert.Equal("UTC", time.ZoneName);
    }

    [Fact]
    public void ToUtc_and_ToLocal_round_trip_through_a_zone()
    {
        var time = BrowserTime.From("Europe/London");
        var local = new DateTime(2026, 7, 1, 9, 0, 0);

        var utc = time.ToUtc(local);

        Assert.Equal(new DateTime(2026, 7, 1, 8, 0, 0), utc);
        Assert.Equal(local, time.ToLocal(utc));
    }

    [Fact]
    public void ToUtc_reads_a_skipped_hour_as_the_hour_after()
    {
        var time = BrowserTime.From("Europe/London");

        var utc = time.ToUtc(new DateTime(2026, 3, 29, 1, 30, 0));

        Assert.Equal(new DateTime(2026, 3, 29, 1, 30, 0), utc);
    }
}
