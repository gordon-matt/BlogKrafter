namespace BlogKrafter.Tests.Posts;

public class BlogReaderTests
{
    [Fact]
    public async Task ListPostsAsync_shows_only_published_posts_whose_date_has_arrived()
    {
        using var db = new TestDatabase();
        await db.AddPostAsync("Live");
        await db.AddPostAsync("Draft", PostStatus.Draft);
        await db.AddPostAsync("Tomorrow", publishedAt: db.Clock.UtcNow.AddDays(1));

        var today = await db.CreateReader().ListPostsAsync(new PostListQuery());
        db.Clock.Advance(TimeSpan.FromDays(2));
        var later = await db.CreateReader().ListPostsAsync(new PostListQuery());

        Assert.Equal(["Live"], today.Value.Posts.Items.Select(post => post.Title));
        Assert.Equal(["Tomorrow", "Live"], later.Value.Posts.Items.Select(post => post.Title));
    }

    [Fact]
    public async Task ListPostsAsync_pages_newest_first()
    {
        using var db = new TestDatabase();
        db.Options.PageSize = 2;
        foreach (string title in new[] { "One", "Two", "Three" })
        {
            await db.AddPostAsync(title);
            db.Clock.Advance(TimeSpan.FromHours(1));
        }

        var second = await db.CreateReader().ListPostsAsync(new PostListQuery { Page = 2 });

        Assert.Equal(3, second.Value.Posts.TotalCount);
        Assert.Equal(["One"], second.Value.Posts.Items.Select(post => post.Title));
        Assert.False(second.Value.Posts.HasNext);
    }

    [Fact]
    public async Task ListPostsAsync_narrows_to_a_tag_and_counts_its_posts()
    {
        using var db = new TestDatabase();
        await db.AddPostAsync("Tagged", tags: ["Blazor"]);
        await db.AddPostAsync("Untagged");
        await db.AddPostAsync("Hidden draft", PostStatus.Draft, tags: ["Blazor"]);

        var result = await db.CreateReader().ListPostsAsync(new PostListQuery { TagSlug = "BLAZOR" });

        Assert.Equal("Blazor", result.Value.Tag!.Name);
        Assert.Equal(1, result.Value.Tag.PostCount);
        Assert.Equal(["Tagged"], result.Value.Posts.Items.Select(post => post.Title));
    }

    [Fact]
    public async Task ListPostsAsync_returns_not_found_for_an_unknown_category()
    {
        using var db = new TestDatabase();

        var result = await db.CreateReader().ListPostsAsync(new PostListQuery { CategorySlug = "missing" });

        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task ListPostsAsync_searches_titles_and_bodies()
    {
        using var db = new TestDatabase();
        await db.AddPostAsync("Gardening", content: "All about tomatoes.");
        await db.AddPostAsync("Cooking", content: "Soup recipes.");

        var result = await db.CreateReader().ListPostsAsync(new PostListQuery { Search = "TOMATO" });

        Assert.Equal(["Gardening"], result.Value.Posts.Items.Select(post => post.Title));
    }

    [Fact]
    public async Task GetPostAsync_returns_the_post_with_its_neighbours()
    {
        using var db = new TestDatabase();
        await db.AddPostAsync("Older");
        db.Clock.Advance(TimeSpan.FromHours(1));
        await db.AddPostAsync("Middle");
        db.Clock.Advance(TimeSpan.FromHours(1));
        await db.AddPostAsync("Newer");

        var result = await db.CreateReader().GetPostAsync("middle");

        var post = result.Value.Post!;
        Assert.Equal("Middle", post.Title);
        Assert.Equal("older", post.Previous!.Slug);
        Assert.Equal("newer", post.Next!.Slug);
    }

    [Fact]
    public async Task GetPostAsync_hides_a_draft_from_readers()
    {
        using var db = new TestDatabase();
        await db.AddPostAsync("Secret", PostStatus.Draft);

        var result = await db.CreateReader().GetPostAsync("secret");

        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task GetPostAsync_preview_requires_a_signed_in_user()
    {
        using var db = new TestDatabase();
        await db.AddPostAsync("Secret", PostStatus.Draft);

        var anonymous = await db.CreateReader().GetPostAsync("secret", preview: true);
        var editor = await db.CreateReader(new TestUserContext()).GetPostAsync("secret", preview: true);

        Assert.Equal(ResultStatus.Forbidden, anonymous.Status);
        Assert.Equal(PostState.Draft, editor.Value.Post!.State);
    }

    [Fact]
    public async Task GetPostAsync_lists_headings_once_there_are_enough()
    {
        using var db = new TestDatabase();
        db.Options.TableOfContentsMinHeadings = 3;
        await db.AddPostAsync("Two headings", content: "## A\n\n## B");
        await db.AddPostAsync("Three headings", content: "## A\n\n## B\n\n### C");

        var two = await db.CreateReader().GetPostAsync("two-headings");
        var three = await db.CreateReader().GetPostAsync("three-headings");

        Assert.Empty(two.Value.Post!.TableOfContents);
        Assert.Equal(["a", "b", "c"], three.Value.Post!.TableOfContents.Select(entry => entry.Id));
    }

    [Fact]
    public async Task GetPostAsync_ranks_related_posts_by_shared_tags()
    {
        using var db = new TestDatabase();
        await db.AddPostAsync("Subject", tags: ["a", "b"]);
        await db.AddPostAsync("Shares one", tags: ["a"]);
        await db.AddPostAsync("Shares two", tags: ["a", "b"]);
        await db.AddPostAsync("Shares none", tags: ["c"]);

        var result = await db.CreateReader().GetPostAsync("subject");

        Assert.Equal(["shares-two", "shares-one"], result.Value.Post!.Related.Select(post => post.Slug));
    }

    [Fact]
    public async Task GetTagsAsync_counts_only_posts_readers_can_see()
    {
        using var db = new TestDatabase();
        await db.AddPostAsync("One", tags: ["Common", "Rare"]);
        await db.AddPostAsync("Two", tags: ["Common"]);
        await db.AddPostAsync("Draft", PostStatus.Draft, tags: ["Unused"]);

        var tags = await db.CreateReader().GetTagsAsync();

        Assert.Equal([("Common", 2), ("Rare", 1)], tags.Value.Select(tag => (tag.Name, tag.PostCount)));
    }

    [Fact]
    public async Task GetArchiveAsync_groups_posts_by_year()
    {
        using var db = new TestDatabase();
        await db.AddPostAsync("Last year", publishedAt: new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc));
        await db.AddPostAsync("This year");

        var archive = await db.CreateReader().GetArchiveAsync();

        Assert.Equal([2026, 2025], archive.Value.Select(year => year.Year));
    }

    [Fact]
    public async Task GetArchiveAsync_dates_posts_in_the_blogs_time_zone()
    {
        using var db = new TestDatabase();
        db.Options.TimeZone = "Australia/Adelaide";
        await db.AddPostAsync("New year", publishedAt: new DateTime(2025, 12, 31, 14, 0, 0, DateTimeKind.Utc));

        var archive = await db.CreateReader().GetArchiveAsync();

        var year = Assert.Single(archive.Value);
        Assert.Equal(2026, year.Year);
        Assert.Equal(new DateTime(2026, 1, 1, 0, 30, 0), Assert.Single(year.Posts).PublishedAt);
    }

    [Fact]
    public async Task GetSitemapEntriesAsync_lists_the_index_posts_and_used_categories()
    {
        using var db = new TestDatabase();
        var news = await db.AddCategoryAsync("News");
        await db.AddCategoryAsync("Empty");
        await db.AddPostAsync("Story", categoryId: news.Id, tags: ["Local"]);

        var entries = await db.CreateReader().GetSitemapEntriesAsync();

        Assert.Equal(["/blog", "/blog/story", "/blog/category/news", "/blog/tag/local"], entries.Value.Select(entry => entry.Path));
    }

    [Fact]
    public async Task GetFeedItemsAsync_includes_the_full_body()
    {
        using var db = new TestDatabase();
        await db.AddPostAsync("Feed me", content: "Body **text**.");

        var items = await db.CreateReader().GetFeedItemsAsync();

        Assert.Contains("<strong>text</strong>", Assert.Single(items.Value).ContentHtml);
    }

    [Fact]
    public async Task Readers_see_only_their_own_sites_posts()
    {
        using var db = new TestDatabase();
        db.Tenant.TenantId = "site-a";
        await db.AddPostAsync("Shared slug", tags: ["Shared"]);
        db.Tenant.TenantId = "site-b";
        await db.AddPostAsync("Shared slug", tags: ["Shared"], content: "Site B");

        var list = await db.CreateReader().ListPostsAsync(new PostListQuery());
        var post = await db.CreateReader().GetPostAsync("shared-slug");
        var tags = await db.CreateReader().GetTagsAsync();

        Assert.Single(list.Value.Posts.Items);
        Assert.Contains("Site B", post.Value.Post!.ContentHtml);
        Assert.Equal(1, Assert.Single(tags.Value).PostCount);
    }
}