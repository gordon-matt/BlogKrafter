using BlogKrafter.Feeds;
using Microsoft.Data.Sqlite;

namespace BlogKrafter.Tests.Infrastructure;

/// <summary>
/// A private, freshly migrated in-memory SQLite database with the services wired to it, so each test
/// runs against the real schema and can run in parallel with the others.
/// </summary>
public sealed class TestDatabase : IDisposable
{
    private readonly SqliteConnection keepAlive;

    public TestDatabase()
    {
        string connectionString = $"Data Source=blogkrafter-tests-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";

        // The open connection keeps the shared in-memory database alive between contexts.
        keepAlive = new SqliteConnection(connectionString);
        keepAlive.Open();

        Factory = new Data.Sqlite.BlogKrafterDbContextFactory(connectionString);
        using var context = Factory.GetContext();
        context.Database.Migrate();
    }

    public IBlogKrafterDbContextFactory Factory { get; }

    public BlogKrafterOptions Options { get; } = new();

    public TestClock Clock { get; } = new(new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc));

    public TestTenant Tenant { get; } = new();

    public BlogUrls Urls => new(Microsoft.Extensions.Options.Options.Create(Options));

    public IBlogContentRenderer Renderer => new BlogContentRenderer(new BlogHtmlSanitizer(), Microsoft.Extensions.Options.Options.Create(Options));

    public BlogPostService CreatePostService(IUserContextService? user = null) => new(
        NullLogger<BlogPostService>.Instance,
        user ?? new TestUserContext(),
        Tenant,
        Renderer,
        Microsoft.Extensions.Options.Options.Create(Options),
        Repository<BlogPost>(),
        Repository<BlogPostTag>(),
        Repository<BlogTag>(),
        Repository<BlogCategory>(),
        Repository<BlogSlugRedirect>(),
        Clock);

    public BlogReader CreateReader(IUserContextService? user = null) => new(
        user ?? TestUserContext.Anonymous,
        Tenant,
        Renderer,
        Microsoft.Extensions.Options.Options.Create(Options),
        Urls,
        new BlogTimeZone(Microsoft.Extensions.Options.Options.Create(Options)),
        Repository<BlogPost>(),
        Repository<BlogPostTag>(),
        Repository<BlogCategory>(),
        Repository<BlogTag>(),
        Repository<BlogSlugRedirect>(),
        Clock);

    public BlogCategoryService CreateCategoryService(IUserContextService? user = null) => new(
        NullLogger<BlogCategoryService>.Instance,
        user ?? new TestUserContext(),
        Tenant,
        Repository<BlogCategory>(),
        Repository<BlogPost>());

    public BlogTagService CreateTagService(IUserContextService? user = null) => new(
        NullLogger<BlogTagService>.Instance,
        user ?? new TestUserContext(),
        Tenant,
        Repository<BlogTag>(),
        Repository<BlogPostTag>());

    public BlogFeedWriter CreateFeedWriter() => new(Microsoft.Extensions.Options.Options.Create(Options), Urls, Clock);

    public IRepository<T> Repository<T>() where T : class, IEntity => new EntityFrameworkRepository<T>(Factory);

    /// <summary>Saves a post through the service and fails the test if the service refuses it.</summary>
    public async Task<PostEditDto> AddPostAsync(
        string title,
        PostStatus status = PostStatus.Published,
        string content = "Some words worth reading.",
        DateTime? publishedAt = null,
        int? categoryId = null,
        string? slug = null,
        params string[] tags)
    {
        var result = await CreatePostService().SaveAsync(new SavePostRequest
        {
            Title = title,
            Slug = slug,
            Content = content,
            ContentFormat = ContentFormat.Markdown,
            Status = status,
            PublishedAt = publishedAt,
            CategoryId = categoryId,
            Tags = tags,
        });

        Assert.True(result.IsSuccess, string.Join("; ", result.Errors.Concat(result.ValidationErrors.Select(error => error.ErrorMessage))));
        return result.Value;
    }

    public async Task<CategoryDto> AddCategoryAsync(string name)
    {
        var result = await CreateCategoryService().SaveAsync(new SaveCategoryRequest { Name = name });
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    public void Dispose() => keepAlive.Dispose();
}
