namespace BlogKrafter.Tests.Posts;

public class BlogPostServiceTests
{
    [Fact]
    public async Task SaveAsync_creates_a_post_for_the_current_user()
    {
        using var db = new TestDatabase();

        var result = await db.CreatePostService().SaveAsync(new SavePostRequest
        {
            Title = "  Hello, World!  ",
            Content = "# Heading\n\nFirst paragraph.",
            ContentFormat = ContentFormat.Markdown,
            Status = PostStatus.Published,
        });

        Assert.Equal(ResultStatus.Ok, result.Status);
        Assert.Equal("Hello, World!", result.Value.Title);
        Assert.Equal("hello-world", result.Value.Slug);
        Assert.Equal(PostState.Published, result.Value.State);
        Assert.Equal(db.Clock.UtcNow, result.Value.PublishedAt);
        Assert.Equal("Ada Editor", result.Value.AuthorName);

        await using var context = db.Factory.GetContext();
        var stored = await context.Set<BlogPost>().SingleAsync();
        Assert.Equal("editor-1", stored.AuthorUserId);
        Assert.Contains("<h1>Heading</h1>", stored.ContentHtml);
        Assert.Equal("First paragraph.", stored.Excerpt);
        Assert.Equal(1, stored.ReadingTimeMinutes);
    }

    [Fact]
    public async Task SaveAsync_returns_forbidden_without_a_signed_in_user()
    {
        using var db = new TestDatabase();

        var result = await db.CreatePostService(TestUserContext.Anonymous).SaveAsync(new SavePostRequest { Title = "Nope" });

        Assert.Equal(ResultStatus.Forbidden, result.Status);
    }

    [Fact]
    public async Task SaveAsync_reports_every_invalid_field()
    {
        using var db = new TestDatabase();

        var result = await db.CreatePostService().SaveAsync(new SavePostRequest
        {
            Title = " ",
            Status = PostStatus.Published,
            CoverImageUrl = "javascript:alert(1)",
            CanonicalUrl = "/relative",
            ContentFormat = (ContentFormat)42,
        });

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Equal(
            [nameof(SavePostRequest.CanonicalUrl), nameof(SavePostRequest.Content), nameof(SavePostRequest.ContentFormat),
             nameof(SavePostRequest.CoverImageUrl), nameof(SavePostRequest.Title)],
            result.ValidationErrors.Select(error => error.Identifier).Order());
    }

    [Fact]
    public async Task SaveAsync_allows_a_draft_without_content()
    {
        using var db = new TestDatabase();

        var post = await db.AddPostAsync("An idea", PostStatus.Draft, content: string.Empty);

        Assert.Equal(PostState.Draft, post.State);
        Assert.Null(post.PublishedAt);
    }

    [Fact]
    public async Task SaveAsync_numbers_a_title_slug_that_is_already_taken()
    {
        using var db = new TestDatabase();

        await db.AddPostAsync("Release notes");
        var second = await db.AddPostAsync("Release notes");
        var third = await db.AddPostAsync("Release notes");

        Assert.Equal("release-notes-2", second.Slug);
        Assert.Equal("release-notes-3", third.Slug);
    }

    [Fact]
    public async Task SaveAsync_steers_a_title_slug_away_from_the_blog_routes()
    {
        using var db = new TestDatabase();

        var post = await db.AddPostAsync("Archive");

        Assert.Equal("archive-2", post.Slug);
    }

    [Theory]
    [InlineData("archive")]
    [InlineData("Feeds")]
    public async Task SaveAsync_refuses_an_explicit_slug_the_blog_uses(string slug)
    {
        using var db = new TestDatabase();

        var result = await db.CreatePostService().SaveAsync(new SavePostRequest { Title = "Post", Slug = slug });

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Equal(nameof(SavePostRequest.Slug), Assert.Single(result.ValidationErrors).Identifier);
    }

    [Fact]
    public async Task SaveAsync_refuses_an_explicit_slug_another_post_uses()
    {
        using var db = new TestDatabase();
        await db.AddPostAsync("First", slug: "taken");

        var result = await db.CreatePostService().SaveAsync(new SavePostRequest { Title = "Second", Slug = "Taken" });

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Equal(nameof(SavePostRequest.Slug), Assert.Single(result.ValidationErrors).Identifier);
    }

    [Fact]
    public async Task SaveAsync_refuses_a_category_that_does_not_exist()
    {
        using var db = new TestDatabase();

        var result = await db.CreatePostService().SaveAsync(new SavePostRequest { Title = "Post", CategoryId = 99 });

        Assert.Equal(nameof(SavePostRequest.CategoryId), Assert.Single(result.ValidationErrors).Identifier);
    }

    [Fact]
    public async Task SaveAsync_returns_not_found_for_a_missing_post()
    {
        using var db = new TestDatabase();

        var result = await db.CreatePostService().SaveAsync(new SavePostRequest { Id = 99, Title = "Post" });

        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task SaveAsync_keeps_the_original_publish_date_on_later_edits()
    {
        using var db = new TestDatabase();
        var post = await db.AddPostAsync("Dated");
        var firstPublished = post.PublishedAt;

        db.Clock.Advance(TimeSpan.FromDays(3));
        var updated = await db.CreatePostService().SaveAsync(Edit(post) with { Title = "Dated, revised" });

        Assert.Equal(firstPublished, updated.Value.PublishedAt);
        Assert.Equal(db.Clock.UtcNow, updated.Value.UpdatedAt);
    }

    [Fact]
    public async Task SaveAsync_schedules_a_post_with_a_future_date()
    {
        using var db = new TestDatabase();

        var post = await db.AddPostAsync("Coming soon", publishedAt: db.Clock.UtcNow.AddDays(1));

        Assert.Equal(PostState.Scheduled, post.State);
    }

    [Fact]
    public async Task SaveAsync_creates_and_reuses_tags_by_slug()
    {
        using var db = new TestDatabase();
        await db.AddPostAsync("One", tags: ["Blazor"]);

        var post = await db.AddPostAsync("Two", tags: ["blazor", " .NET ", "Blazor!", "  "]);

        Assert.Equal([".NET", "Blazor"], post.Tags);
        await using var context = db.Factory.GetContext();
        Assert.Equal(["blazor", "net"], await context.Set<BlogTag>().Select(tag => tag.Slug).OrderBy(slug => slug).ToListAsync());
    }

    [Fact]
    public async Task SaveAsync_replaces_the_tags_of_an_existing_post()
    {
        using var db = new TestDatabase();
        var post = await db.AddPostAsync("Tagged", tags: ["One", "Two"]);

        var updated = await db.CreatePostService().SaveAsync(Edit(post) with { Tags = ["Two", "Three"] });

        Assert.Equal(["Three", "Two"], updated.Value.Tags);
    }

    [Fact]
    public async Task SaveAsync_leaves_a_redirect_when_a_live_post_moves()
    {
        using var db = new TestDatabase();
        var post = await db.AddPostAsync("Original title");

        await db.CreatePostService().SaveAsync(Edit(post) with { Slug = "new-address" });

        var lookup = await db.CreateReader().GetPostAsync("original-title");
        Assert.Null(lookup.Value.Post);
        Assert.Equal("new-address", lookup.Value.RedirectSlug);
    }

    [Fact]
    public async Task SaveAsync_leaves_no_redirect_when_a_draft_moves()
    {
        using var db = new TestDatabase();
        var post = await db.AddPostAsync("Draft title", PostStatus.Draft);

        await db.CreatePostService().SaveAsync(Edit(post) with { Slug = "better-title" });

        await using var context = db.Factory.GetContext();
        Assert.Empty(await context.Set<BlogSlugRedirect>().ToListAsync());
    }

    [Fact]
    public async Task SaveAsync_drops_the_redirect_when_a_post_moves_back()
    {
        using var db = new TestDatabase();
        var post = await db.AddPostAsync("Round trip");
        var service = db.CreatePostService();

        var moved = await service.SaveAsync(Edit(post) with { Slug = "elsewhere" });
        await service.SaveAsync(Edit(moved.Value) with { Slug = "round-trip" });

        await using var context = db.Factory.GetContext();
        var redirect = Assert.Single(await context.Set<BlogSlugRedirect>().ToListAsync());
        Assert.Equal("elsewhere", redirect.Slug);
    }

    [Fact]
    public async Task SaveAsync_will_not_reuse_an_address_that_still_redirects()
    {
        using var db = new TestDatabase();
        var post = await db.AddPostAsync("First home");
        await db.CreatePostService().SaveAsync(Edit(post) with { Slug = "second-home" });

        var newcomer = await db.AddPostAsync("First home");

        Assert.Equal("first-home-2", newcomer.Slug);
    }

    [Fact]
    public async Task ListAsync_filters_by_state_and_search()
    {
        using var db = new TestDatabase();
        await db.AddPostAsync("Live post");
        await db.AddPostAsync("Draft post", PostStatus.Draft);
        await db.AddPostAsync("Later post", publishedAt: db.Clock.UtcNow.AddDays(2));
        var service = db.CreatePostService();

        var scheduled = await service.ListAsync(new AdminPostQuery { Filter = PostFilter.Scheduled });
        var drafts = await service.ListAsync(new AdminPostQuery { Filter = PostFilter.Drafts });
        var search = await service.ListAsync(new AdminPostQuery { Search = "LIVE" });

        Assert.Equal("Later post", Assert.Single(scheduled.Value.Items).Title);
        Assert.Equal("Draft post", Assert.Single(drafts.Value.Items).Title);
        Assert.Equal("Live post", Assert.Single(search.Value.Items).Title);
    }

    [Fact]
    public async Task ListAsync_pages_the_results()
    {
        using var db = new TestDatabase();
        for (int i = 1; i <= 5; i++)
        {
            await db.AddPostAsync($"Post {i}");
            db.Clock.Advance(TimeSpan.FromMinutes(1));
        }

        var page = await db.CreatePostService().ListAsync(new AdminPostQuery { Page = 2, PageSize = 2 });

        Assert.Equal(5, page.Value.TotalCount);
        Assert.Equal(3, page.Value.PageCount);
        Assert.Equal(["Post 3", "Post 2"], page.Value.Items.Select(item => item.Title));
    }

    [Fact]
    public async Task DeleteAsync_removes_the_post_and_its_links()
    {
        using var db = new TestDatabase();
        var post = await db.AddPostAsync("Short lived", tags: ["Temp"]);
        await db.CreatePostService().SaveAsync(Edit(post) with { Slug = "moved" });

        var result = await db.CreatePostService().DeleteAsync(post.Id);

        Assert.True(result.IsSuccess);
        await using var context = db.Factory.GetContext();
        Assert.Empty(await context.Set<BlogPost>().ToListAsync());
        Assert.Empty(await context.Set<BlogPostTag>().ToListAsync());
        Assert.Empty(await context.Set<BlogSlugRedirect>().ToListAsync());
        Assert.Single(await context.Set<BlogTag>().ToListAsync());
    }

    [Fact]
    public async Task DeleteAsync_cannot_reach_another_sites_post()
    {
        using var db = new TestDatabase();
        db.Tenant.TenantId = "site-a";
        var post = await db.AddPostAsync("Site A post");

        db.Tenant.TenantId = "site-b";
        var result = await db.CreatePostService().DeleteAsync(post.Id);

        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    private static SavePostRequest Edit(PostEditDto post) => new()
    {
        Id = post.Id,
        Title = post.Title,
        Slug = post.Slug,
        Summary = post.Summary,
        ContentFormat = post.ContentFormat,
        Content = post.Content,
        Status = post.Status,
        PublishedAt = post.PublishedAt,
        CategoryId = post.CategoryId,
        Tags = [.. post.Tags],
    };
}