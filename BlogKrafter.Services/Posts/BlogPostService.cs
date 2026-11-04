namespace BlogKrafter.Posts;

public sealed class BlogPostService(
    ILogger<BlogPostService> logger,
    IUserContextService userContext,
    IBlogTenantProvider tenants,
    IBlogContentRenderer renderer,
    IOptions<BlogKrafterOptions> options,
    IRepository<BlogPost> posts,
    IRepository<BlogPostTag> postTags,
    IRepository<BlogTag> tags,
    IRepository<BlogCategory> categories,
    IRepository<BlogSlugRedirect> redirects,
    TimeProvider time) : IBlogPostService
{
    public async Task<Result<PagedList<AdminPostRow>>> ListAsync(AdminPostQuery query, CancellationToken cancellationToken = default)
    {
        if (userContext.GetCurrentUserId() is null)
        {
            return Result<PagedList<AdminPostRow>>.Forbidden();
        }

        string tenantId = tenants.GetTenantId();
        var now = Now();
        int page = Math.Max(1, query.Page);
        int pageSize = Math.Clamp(query.PageSize, 1, 200);

        var filter = query.Filter switch
        {
            PostFilter.Drafts => post => post.TenantId == tenantId && post.Status == PostStatus.Draft,
            PostFilter.Scheduled => post => post.TenantId == tenantId && post.Status == PostStatus.Published
                && (post.PublishedAt == null || post.PublishedAt > now),
            PostFilter.Published => PostQueries.Visible(tenantId, now),
            _ => post => post.TenantId == tenantId,
        };

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string term = query.Search.Trim().ToLowerInvariant();
            filter = filter.And(post => post.Title.ToLower().Contains(term) || post.Slug.Contains(term));
        }

        var found = await posts.FindAsync(
            new SearchOptions<BlogPost>
            {
                Query = filter,
                OrderBy = items => items.OrderByDescending(post => post.UpdatedAt).ThenByDescending(post => post.Id),
                PageNumber = page,
                PageSize = pageSize,
                CancellationToken = cancellationToken,
            },
            post => new AdminPostRow(
                post.Id,
                post.Title,
                post.Slug,
                post.Status == PostStatus.Draft
                    ? PostState.Draft
                    : post.PublishedAt == null || post.PublishedAt > now ? PostState.Scheduled : PostState.Published,
                post.PublishedAt,
                post.UpdatedAt,
                post.AuthorName,
                post.Category == null ? null : post.Category.Name,
                post.IsFeatured));

        return Result<PagedList<AdminPostRow>>.Success(new PagedList<AdminPostRow>([.. found], page, pageSize, found.ItemCount));
    }

    public async Task<Result<PostEditDto>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        if (userContext.GetCurrentUserId() is null)
        {
            return Result<PostEditDto>.Forbidden();
        }

        string tenantId = tenants.GetTenantId();
        var post = await posts.FindOneAsync(new SearchOptions<BlogPost>
        {
            Query = item => item.Id == id && item.TenantId == tenantId,
            Include = query => query.Include(item => item.Tags).ThenInclude(link => link.Tag),
            CancellationToken = cancellationToken,
        });

        return post is null
            ? Result<PostEditDto>.NotFound($"Post {id} was not found.")
            : Result<PostEditDto>.Success(ToEditDto(post));
    }

    public async Task<Result<PostEditDto>> SaveAsync(SavePostRequest request, CancellationToken cancellationToken = default)
    {
        if (userContext.GetCurrentUserId() is not string userId)
        {
            return Result<PostEditDto>.Forbidden();
        }

        string tenantId = tenants.GetTenantId();
        if (tenantId.Length > Constants.MaxLengths.TenantId)
        {
            return Result<PostEditDto>.Error($"The tenant id is longer than {Constants.MaxLengths.TenantId} characters.");
        }

        var validated = Validate(request);
        if (!validated.IsSuccess)
        {
            return validated.To<PostEditDto>();
        }

        var input = validated.Value;

        if (request.CategoryId is int categoryId
            && !await categories.ExistsAsync(
                item => item.Id == categoryId && item.TenantId == tenantId,
                ContextOptions.ForCancellationToken(cancellationToken)))
        {
            return Result<PostEditDto>.Invalid(new ValidationError(nameof(request.CategoryId), "That category no longer exists."));
        }

        BlogPost? existing = null;
        if (request.Id is int postId)
        {
            existing = await posts.FindOneAsync(new SearchOptions<BlogPost>
            {
                Query = item => item.Id == postId && item.TenantId == tenantId,
                CancellationToken = cancellationToken,
            });

            if (existing is null)
            {
                return Result<PostEditDto>.NotFound($"Post {postId} was not found.");
            }
        }

        var slug = await ResolveSlugAsync(tenantId, input.Slug, input.Title, existing?.Id, cancellationToken);
        if (!slug.IsSuccess)
        {
            return slug.To<PostEditDto>();
        }

        var rendered = renderer.Render(request.Content, request.ContentFormat);
        var now = Now();

        DateTime? publishedAt = request.PublishedAt is DateTime requested
            ? DateTime.SpecifyKind(requested, DateTimeKind.Utc)
            : request.Status == PostStatus.Published ? existing?.PublishedAt ?? now : null;

        var post = existing ?? new BlogPost
        {
            TenantId = tenantId,
            AuthorUserId = userId,
            AuthorName = Truncate(userContext.GetCurrentUserName() ?? userId, Constants.MaxLengths.AuthorName),
            CreatedAt = now,
        };

        string previousSlug = post.Slug;
        bool wasLive = existing is not null && PostQueries.StateOf(existing.Status, existing.PublishedAt, now) == PostState.Published;

        post.Title = input.Title;
        post.Slug = slug.Value;
        post.Summary = input.Summary;
        post.Excerpt = input.Summary ?? rendered.Excerpt;
        post.ContentFormat = request.ContentFormat;
        post.Content = request.Content ?? string.Empty;
        post.ContentHtml = rendered.Html;
        post.ReadingTimeMinutes = ReadingMinutes(rendered.WordCount);
        post.CoverImageUrl = input.CoverImageUrl;
        post.CoverImageAlt = Blank(request.CoverImageAlt);
        post.MetaTitle = Blank(request.MetaTitle);
        post.MetaDescription = Blank(request.MetaDescription);
        post.CanonicalUrl = input.CanonicalUrl;
        post.Status = request.Status;
        post.PublishedAt = publishedAt;
        post.IsFeatured = request.IsFeatured;
        post.CommentsEnabled = request.CommentsEnabled;
        post.CategoryId = request.CategoryId;
        post.UpdatedAt = now;

        var context = ContextOptions.ForCancellationToken(cancellationToken);
        if (existing is null)
        {
            post = await posts.InsertAsync(post, context);
            logger.LogInformation("User {UserId} created blog post {PostId}", userId, post.Id);
        }
        else
        {
            post = await posts.UpdateAsync(post, context);
            logger.LogInformation("User {UserId} updated blog post {PostId}", userId, post.Id);
        }

        await KeepOldAddressAsync(tenantId, post.Id, wasLive ? previousSlug : null, post.Slug, now, cancellationToken);
        await ReplaceTagsAsync(tenantId, post.Id, input.Tags, cancellationToken);

        return await GetAsync(post.Id, cancellationToken);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (userContext.GetCurrentUserId() is not string userId)
        {
            return Result.Forbidden();
        }

        string tenantId = tenants.GetTenantId();

        // Tag links and old-slug redirects go with the post through the foreign keys' cascade.
        int deleted = await posts.DeleteAsync(
            post => post.Id == id && post.TenantId == tenantId,
            ContextOptions.ForCancellationToken(cancellationToken));

        if (deleted == 0)
        {
            return Result.NotFound($"Post {id} was not found.");
        }

        logger.LogInformation("User {UserId} deleted blog post {PostId}", userId, id);
        return Result.Success();
    }

    /// <summary>
    /// An explicit slug must be free; one derived from the title gets a numeric suffix until it is.
    /// A slug counts as taken when another post uses it now or used to and still redirects from it.
    /// </summary>
    private async Task<Result<string>> ResolveSlugAsync(
        string tenantId,
        string? requested,
        string title,
        int? postId,
        CancellationToken cancellationToken)
    {
        if (requested is not null)
        {
            return await IsTakenAsync(tenantId, requested, postId, cancellationToken)
                ? Result<string>.Invalid(new ValidationError(nameof(SavePostRequest.Slug), "Another post already uses this address."))
                : Result<string>.Success(requested);
        }

        string baseSlug = Slugs.Generate(title);
        if (baseSlug.Length == 0)
        {
            baseSlug = "post";
        }

        string candidate = baseSlug;
        for (int n = 2; BlogUrls.ReservedSlugs.Contains(candidate) || await IsTakenAsync(tenantId, candidate, postId, cancellationToken); n++)
        {
            candidate = Slugs.WithSuffix(baseSlug, n);
        }

        return Result<string>.Success(candidate);
    }

    private async Task<bool> IsTakenAsync(string tenantId, string slug, int? postId, CancellationToken cancellationToken)
    {
        int self = postId ?? 0;
        var context = ContextOptions.ForCancellationToken(cancellationToken);

        return await posts.ExistsAsync(post => post.TenantId == tenantId && post.Slug == slug && post.Id != self, context)
            || await redirects.ExistsAsync(redirect => redirect.TenantId == tenantId && redirect.Slug == slug && redirect.PostId != self, context);
    }

    /// <summary>
    /// When a live post changes address, the old one becomes a redirect. A post renamed back to an
    /// address it used before no longer needs that redirect, since the slug is its own again.
    /// </summary>
    private async Task KeepOldAddressAsync(
        string tenantId,
        int postId,
        string? oldSlug,
        string newSlug,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var context = ContextOptions.ForCancellationToken(cancellationToken);

        await redirects.DeleteAsync(
            redirect => redirect.TenantId == tenantId && redirect.PostId == postId && redirect.Slug == newSlug,
            context);

        if (string.IsNullOrEmpty(oldSlug) || string.Equals(oldSlug, newSlug, StringComparison.Ordinal))
        {
            return;
        }

        if (!await redirects.ExistsAsync(redirect => redirect.TenantId == tenantId && redirect.Slug == oldSlug, context))
        {
            await redirects.InsertAsync(
                new BlogSlugRedirect { TenantId = tenantId, Slug = oldSlug, PostId = postId, CreatedAt = now },
                context);
        }
    }

    private async Task ReplaceTagsAsync(string tenantId, int postId, IReadOnlyList<(string Name, string Slug)> wanted, CancellationToken cancellationToken)
    {
        var context = ContextOptions.ForCancellationToken(cancellationToken);
        await postTags.DeleteAsync(link => link.PostId == postId, context);

        if (wanted.Count == 0)
        {
            return;
        }

        var slugs = wanted.Select(tag => tag.Slug).ToList();
        var existing = await tags.FindAsync(new SearchOptions<BlogTag>
        {
            Query = tag => tag.TenantId == tenantId && slugs.Contains(tag.Slug),
            CancellationToken = cancellationToken,
        });

        var idBySlug = existing.ToDictionary(tag => tag.Slug, tag => tag.Id, StringComparer.Ordinal);
        foreach (var (name, slug) in wanted.Where(tag => !idBySlug.ContainsKey(tag.Slug)))
        {
            var created = await tags.InsertAsync(new BlogTag { TenantId = tenantId, Name = name, Slug = slug }, context);
            idBySlug[slug] = created.Id;
        }

        await postTags.InsertAsync(
            [.. slugs.Select(slug => new BlogPostTag { PostId = postId, TagId = idBySlug[slug] })],
            context);
    }

    private Result<ValidatedPost> Validate(SavePostRequest? request)
    {
        if (request is null)
        {
            return Result<ValidatedPost>.Invalid(new ValidationError(nameof(request), "A post is required."));
        }

        var errors = new List<ValidationError>();

        string title = request.Title?.Trim() ?? string.Empty;
        if (title.Length == 0)
        {
            errors.Add(new ValidationError(nameof(request.Title), "Give the post a title."));
        }
        else if (title.Length > Constants.MaxLengths.Title)
        {
            errors.Add(new ValidationError(nameof(request.Title), $"Titles can be at most {Constants.MaxLengths.Title} characters."));
        }

        string? slug = null;
        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            slug = Slugs.Generate(request.Slug);
            if (slug.Length == 0)
            {
                errors.Add(new ValidationError(nameof(request.Slug), "Use letters or numbers in the address."));
            }
            else if (BlogUrls.ReservedSlugs.Contains(slug))
            {
                errors.Add(new ValidationError(nameof(request.Slug), $"'{slug}' is used by the blog itself. Choose another address."));
            }
        }

        if (!Enum.IsDefined(request.ContentFormat))
        {
            errors.Add(new ValidationError(nameof(request.ContentFormat), "Choose Markdown or rich text."));
        }

        if (request.Status == PostStatus.Published && string.IsNullOrWhiteSpace(request.Content))
        {
            errors.Add(new ValidationError(nameof(request.Content), "A published post needs some content."));
        }

        string? summary = Blank(request.Summary);
        AddLengthError(errors, nameof(request.Summary), summary, Constants.MaxLengths.Summary, "Summaries");
        AddLengthError(errors, nameof(request.MetaTitle), Blank(request.MetaTitle), Constants.MaxLengths.Title, "Search titles");
        AddLengthError(errors, nameof(request.MetaDescription), Blank(request.MetaDescription), Constants.MaxLengths.MetaDescription, "Search descriptions");
        AddLengthError(errors, nameof(request.CoverImageAlt), Blank(request.CoverImageAlt), Constants.MaxLengths.ImageAlt, "Image descriptions");

        string? coverImageUrl = Blank(request.CoverImageUrl);
        if (coverImageUrl is not null && (coverImageUrl.Length > Constants.MaxLengths.Url || !IsSafeUrl(coverImageUrl, allowRelative: true)))
        {
            errors.Add(new ValidationError(nameof(request.CoverImageUrl), "Use a site path such as /media/photo.jpg or an http(s) address."));
        }

        string? canonicalUrl = Blank(request.CanonicalUrl);
        if (canonicalUrl is not null && (canonicalUrl.Length > Constants.MaxLengths.Url || !IsSafeUrl(canonicalUrl, allowRelative: false)))
        {
            errors.Add(new ValidationError(nameof(request.CanonicalUrl), "The canonical address must be a full http(s) URL."));
        }

        var tagList = new List<(string Name, string Slug)>();
        foreach (string raw in request.Tags ?? [])
        {
            string name = raw?.Trim() ?? string.Empty;
            string tagSlug = Slugs.Generate(name);
            if (tagSlug.Length == 0)
            {
                continue;
            }

            if (name.Length > Constants.MaxLengths.TagName)
            {
                errors.Add(new ValidationError(nameof(request.Tags), $"Tags can be at most {Constants.MaxLengths.TagName} characters."));
                break;
            }

            if (tagList.All(tag => tag.Slug != tagSlug))
            {
                tagList.Add((name, tagSlug));
            }
        }

        return errors.Count > 0
            ? Result<ValidatedPost>.Invalid(errors)
            : Result<ValidatedPost>.Success(new ValidatedPost(title, slug, summary, coverImageUrl, canonicalUrl, tagList));
    }

    private static bool IsSafeUrl(string url, bool allowRelative) => (allowRelative && url.StartsWith('/') && !url.StartsWith("//", StringComparison.Ordinal)) || (Uri.TryCreate(url, UriKind.Absolute, out var absolute) && absolute.Scheme is "http" or "https");

    private static void AddLengthError(List<ValidationError> errors, string field, string? value, int max, string label)
    {
        if (value is not null && value.Length > max)
        {
            errors.Add(new ValidationError(field, $"{label} can be at most {max} characters."));
        }
    }

    private int ReadingMinutes(int words) =>
        words == 0 ? 0 : Math.Max(1, (int)Math.Ceiling(words / (double)Math.Max(1, options.Value.WordsPerMinute)));

    private PostEditDto ToEditDto(BlogPost post) => new(
        post.Id,
        post.Title,
        post.Slug,
        post.Summary,
        post.ContentFormat,
        post.Content,
        post.CoverImageUrl,
        post.CoverImageAlt,
        post.MetaTitle,
        post.MetaDescription,
        post.CanonicalUrl,
        post.Status,
        PostQueries.StateOf(post.Status, post.PublishedAt, Now()),
        post.PublishedAt,
        post.IsFeatured,
        post.CommentsEnabled,
        post.CategoryId,
        [.. post.Tags.Where(link => link.Tag is not null).Select(link => link.Tag!.Name).Order(StringComparer.OrdinalIgnoreCase)],
        post.AuthorName,
        post.CreatedAt,
        post.UpdatedAt);

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string Truncate(string value, int max) => value.Length <= max ? value : value[..max];

    private DateTime Now() => time.GetUtcNow().UtcDateTime;

    private sealed record ValidatedPost(
        string Title,
        string? Slug,
        string? Summary,
        string? CoverImageUrl,
        string? CanonicalUrl,
        IReadOnlyList<(string Name, string Slug)> Tags);
}