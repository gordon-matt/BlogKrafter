namespace BlogKrafter.Tests.Taxonomy;

public class TaxonomyServiceTests
{
    [Fact]
    public async Task Category_SaveAsync_derives_the_slug_and_refuses_duplicates()
    {
        using var db = new TestDatabase();
        var service = db.CreateCategoryService();

        var first = await service.SaveAsync(new SaveCategoryRequest { Name = "Release Notes" });
        var duplicate = await service.SaveAsync(new SaveCategoryRequest { Name = "Other", Slug = "release-notes" });

        Assert.Equal("release-notes", first.Value.Slug);
        Assert.Equal(nameof(SaveCategoryRequest.Slug), Assert.Single(duplicate.ValidationErrors).Identifier);
    }

    [Fact]
    public async Task Category_ListAsync_counts_drafts_too()
    {
        using var db = new TestDatabase();
        var news = await db.AddCategoryAsync("News");
        await db.AddPostAsync("Live", categoryId: news.Id);
        await db.AddPostAsync("Draft", PostStatus.Draft, categoryId: news.Id);

        var categories = await db.CreateCategoryService().ListAsync();

        Assert.Equal(2, Assert.Single(categories.Value).PostCount);
    }

    [Fact]
    public async Task Category_DeleteAsync_keeps_its_posts_uncategorised()
    {
        using var db = new TestDatabase();
        var news = await db.AddCategoryAsync("News");
        var post = await db.AddPostAsync("Story", categoryId: news.Id);

        var result = await db.CreateCategoryService().DeleteAsync(news.Id);

        Assert.True(result.IsSuccess);
        var reloaded = await db.CreatePostService().GetAsync(post.Id);
        Assert.Null(reloaded.Value.CategoryId);
    }

    [Fact]
    public async Task Category_SaveAsync_returns_forbidden_without_a_signed_in_user()
    {
        using var db = new TestDatabase();

        var result = await db.CreateCategoryService(TestUserContext.Anonymous).SaveAsync(new SaveCategoryRequest { Name = "News" });

        Assert.Equal(ResultStatus.Forbidden, result.Status);
    }

    [Fact]
    public async Task Tag_SearchAsync_puts_prefix_matches_first()
    {
        using var db = new TestDatabase();
        await db.AddPostAsync("Post", tags: ["Learning Blazor", "Blazor", "Razor"]);

        var result = await db.CreateTagService().SearchAsync("bla");

        Assert.Equal(["Blazor", "Learning Blazor"], result.Value.Select(tag => tag.Name));
    }

    [Fact]
    public async Task Tag_RenameAsync_renames_in_place()
    {
        using var db = new TestDatabase();
        var post = await db.AddPostAsync("Post", tags: ["dotnet"]);
        int tagId = Assert.Single((await db.CreateTagService().ListAsync()).Value).Id;

        var result = await db.CreateTagService().RenameAsync(tagId, ".NET");

        Assert.Equal(("net", 1), (result.Value.Slug, result.Value.PostCount));
        Assert.Equal([".NET"], (await db.CreatePostService().GetAsync(post.Id)).Value.Tags);
    }

    [Fact]
    public async Task Tag_RenameAsync_merges_into_a_tag_with_the_same_slug()
    {
        using var db = new TestDatabase();
        var both = await db.AddPostAsync("Both", tags: ["Blazor", "Blazer"]);
        var typo = await db.AddPostAsync("Typo only", tags: ["Blazer"]);
        var tags = (await db.CreateTagService().ListAsync()).Value;
        int typoId = tags.Single(tag => tag.Name == "Blazer").Id;

        var result = await db.CreateTagService().RenameAsync(typoId, "blazor");

        Assert.Equal(("Blazor", 2), (result.Value.Name, result.Value.PostCount));
        Assert.Single((await db.CreateTagService().ListAsync()).Value);
        Assert.Equal(["Blazor"], (await db.CreatePostService().GetAsync(both.Id)).Value.Tags);
        Assert.Equal(["Blazor"], (await db.CreatePostService().GetAsync(typo.Id)).Value.Tags);
    }

    [Fact]
    public async Task Tag_DeleteAsync_unlinks_it_from_posts()
    {
        using var db = new TestDatabase();
        var post = await db.AddPostAsync("Post", tags: ["Gone", "Kept"]);
        int goneId = (await db.CreateTagService().ListAsync()).Value.Single(tag => tag.Name == "Gone").Id;

        var result = await db.CreateTagService().DeleteAsync(goneId);

        Assert.True(result.IsSuccess);
        Assert.Equal(["Kept"], (await db.CreatePostService().GetAsync(post.Id)).Value.Tags);
    }
}