namespace BlogKrafter.Posts;

public sealed class BlogReader(
    IUserContextService userContext,
    IBlogTenantProvider tenants,
    IBlogContentRenderer renderer,
    IOptions<BlogKrafterOptions> options,
    BlogUrls urls,
    BlogTimeZone timeZone,
    IRepository<BlogPost> posts,
    IRepository<BlogPostTag> postTags,
    IRepository<BlogCategory> categories,
    IRepository<BlogTag> tags,
    IRepository<BlogSlugRedirect> redirects,
    TimeProvider time) : IBlogReader
{
    public async Task<Result<PostListResult>> ListPostsAsync(PostListQuery query, CancellationToken cancellationToken = default)
    {
        var scope = await ResolveScopeAsync(query.TagSlug, query.CategorySlug, cancellationToken);
        if (!scope.IsSuccess)
        {
            return scope.To<PostListResult>();
        }

        var (filter, tag, category) = scope.Value;
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string term = query.Search.Trim().ToLowerInvariant();
            filter = filter.And(post =>
                post.Title.ToLower().Contains(term)
                || post.Excerpt.ToLower().Contains(term)
                || post.Content.ToLower().Contains(term));
        }

        int page = Math.Max(1, query.Page);
        int pageSize = Math.Max(1, options.Value.PageSize);

        var found = await posts.FindAsync(
            new SearchOptions<BlogPost>
            {
                Query = filter,
                OrderBy = PostQueries.Newest,
                PageNumber = page,
                PageSize = pageSize,
                CancellationToken = cancellationToken,
            },
            PostQueries.Summary);

        var paged = new PagedList<PostSummaryDto>([.. found], page, pageSize, found.ItemCount);
        return Result<PostListResult>.Success(new PostListResult(
            paged,
            tag is null ? null : tag with { PostCount = found.ItemCount },
            category is null ? null : category with { PostCount = found.ItemCount }));
    }

    public async Task<Result<PostLookup>> GetPostAsync(string slug, bool preview = false, CancellationToken cancellationToken = default)
    {
        if (preview && !userContext.IsAuthenticated())
        {
            return Result<PostLookup>.Forbidden();
        }

        string tenantId = tenants.GetTenantId();
        var now = Now();
        string normalized = (slug ?? string.Empty).Trim().ToLowerInvariant();

        var filter = preview
            ? (post => post.TenantId == tenantId)
            : PostQueries.Visible(tenantId, now);

        var post = await posts.FindOneAsync(new SearchOptions<BlogPost>
        {
            Query = filter.And(item => item.Slug == normalized),
            Include = query => query.Include(item => item.Category).Include(item => item.Tags).ThenInclude(link => link.Tag),
            CancellationToken = cancellationToken,
        });

        if (post is null)
        {
            string? current = await FindRedirectAsync(tenantId, normalized, filter, cancellationToken);
            return current is null
                ? Result<PostLookup>.NotFound($"There is no post at '{normalized}'.")
                : Result<PostLookup>.Success(new PostLookup(null, current));
        }

        var state = PostQueries.StateOf(post.Status, post.PublishedAt, now);
        var headings = renderer.ReadHeadings(post.ContentHtml);
        var tagRefs = post.Tags
            .Where(link => link.Tag is not null)
            .Select(link => new TagRef(link.Tag!.Id, link.Tag.Name, link.Tag.Slug))
            .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        PostLink? previous = null;
        PostLink? next = null;
        IReadOnlyList<PostSummaryDto> related = [];
        if (state == PostState.Published)
        {
            var visible = PostQueries.Visible(tenantId, now);
            var publishedAt = post.PublishedAt!.Value;

            previous = await posts.FindOneAsync(
                new SearchOptions<BlogPost>
                {
                    Query = visible.And(item => item.PublishedAt < publishedAt),
                    OrderBy = PostQueries.Newest,
                    CancellationToken = cancellationToken,
                },
                item => new PostLink(item.Title, item.Slug));

            next = await posts.FindOneAsync(
                new SearchOptions<BlogPost>
                {
                    Query = visible.And(item => item.PublishedAt > publishedAt),
                    OrderBy = query => query.OrderBy(item => item.PublishedAt).ThenBy(item => item.Id),
                    CancellationToken = cancellationToken,
                },
                item => new PostLink(item.Title, item.Slug));

            related = await FindRelatedAsync(post, visible, cancellationToken);
        }

        var dto = new PostDto(
            post.Id,
            post.Title,
            post.Slug,
            post.Excerpt,
            post.Summary,
            post.ContentHtml,
            post.CoverImageUrl,
            post.CoverImageAlt,
            post.MetaTitle,
            post.MetaDescription,
            post.CanonicalUrl,
            state,
            post.PublishedAt,
            post.UpdatedAt,
            post.ReadingTimeMinutes,
            post.AuthorName,
            post.CommentsEnabled,
            post.Category is null ? null : new CategoryRef(post.Category.Id, post.Category.Name, post.Category.Slug),
            tagRefs,
            headings.Count >= options.Value.TableOfContentsMinHeadings ? headings : [],
            previous,
            next,
            related);

        return Result<PostLookup>.Success(new PostLookup(dto, null));
    }

    public async Task<Result<IReadOnlyList<ArchiveYear>>> GetArchiveAsync(CancellationToken cancellationToken = default)
    {
        var entries = await posts.FindAsync(
            new SearchOptions<BlogPost>
            {
                Query = PostQueries.Visible(tenants.GetTenantId(), Now()),
                OrderBy = PostQueries.Newest,
                CancellationToken = cancellationToken,
            },
            post => new ArchiveEntry(post.Title, post.Slug, post.PublishedAt!.Value));

        IReadOnlyList<ArchiveYear> years = [.. entries
            .Select(entry => entry with { PublishedAt = timeZone.ToLocal(entry.PublishedAt) })
            .GroupBy(entry => entry.PublishedAt.Year)
            .OrderByDescending(group => group.Key)
            .Select(group => new ArchiveYear(group.Key, [.. group]))];

        return Result<IReadOnlyList<ArchiveYear>>.Success(years);
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        string tenantId = tenants.GetTenantId();

        var counts = await posts.FindGroupedAsync(
            new SearchOptions<BlogPost>
            {
                Query = PostQueries.Visible(tenantId, Now()).And(post => post.CategoryId != null),
                CancellationToken = cancellationToken,
            },
            post => post.CategoryId!.Value,
            group => new { CategoryId = group.Key, Count = group.Count() });

        var countById = counts.ToDictionary(item => item.CategoryId, item => item.Count);

        var all = await categories.FindAsync(new SearchOptions<BlogCategory>
        {
            Query = item => item.TenantId == tenantId,
            OrderBy = query => query.OrderBy(item => item.SortOrder).ThenBy(item => item.Name),
            CancellationToken = cancellationToken,
        });

        IReadOnlyList<CategoryDto> result = [.. all.Select(item => new CategoryDto(
            item.Id, item.Name, item.Slug, item.Description, item.SortOrder, countById.GetValueOrDefault(item.Id)))];

        return Result<IReadOnlyList<CategoryDto>>.Success(result);
    }

    public async Task<Result<IReadOnlyList<TagDto>>> GetTagsAsync(CancellationToken cancellationToken = default)
    {
        string tenantId = tenants.GetTenantId();

        var counts = await postTags.FindGroupedAsync(
            new SearchOptions<BlogPostTag>
            {
                Query = PostQueries.VisibleLink(tenantId, Now()),
                CancellationToken = cancellationToken,
            },
            link => link.TagId,
            group => new { TagId = group.Key, Count = group.Count() });

        if (counts.Count == 0)
        {
            return Result<IReadOnlyList<TagDto>>.Success([]);
        }

        var countById = counts.ToDictionary(item => item.TagId, item => item.Count);
        var ids = countById.Keys.ToList();

        var used = await tags.FindAsync(new SearchOptions<BlogTag>
        {
            Query = item => item.TenantId == tenantId && ids.Contains(item.Id),
            CancellationToken = cancellationToken,
        });

        IReadOnlyList<TagDto> result = [.. used
            .Select(item => new TagDto(item.Id, item.Name, item.Slug, countById[item.Id]))
            .OrderByDescending(item => item.PostCount)
            .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)];

        return Result<IReadOnlyList<TagDto>>.Success(result);
    }

    public async Task<Result<IReadOnlyList<PostSummaryDto>>> GetRecentPostsAsync(
        int count,
        string? tagSlug = null,
        string? categorySlug = null,
        CancellationToken cancellationToken = default)
    {
        if (count <= 0)
        {
            return Result<IReadOnlyList<PostSummaryDto>>.Success([]);
        }

        var scope = await ResolveScopeAsync(tagSlug, categorySlug, cancellationToken);
        if (!scope.IsSuccess)
        {
            return scope.To<IReadOnlyList<PostSummaryDto>>();
        }

        var found = await posts.FindAsync(
            new SearchOptions<BlogPost>
            {
                Query = scope.Value.Filter,
                OrderBy = PostQueries.Newest,
                PageNumber = 1,
                PageSize = count,
                CancellationToken = cancellationToken,
            },
            PostQueries.Summary);

        return Result<IReadOnlyList<PostSummaryDto>>.Success([.. found]);
    }

    public async Task<Result<IReadOnlyList<FeedItem>>> GetFeedItemsAsync(CancellationToken cancellationToken = default)
    {
        var visible = PostQueries.Visible(tenants.GetTenantId(), Now());

        var summaries = await posts.FindAsync(
            new SearchOptions<BlogPost>
            {
                Query = visible,
                OrderBy = PostQueries.Newest,
                PageNumber = 1,
                PageSize = Math.Max(1, options.Value.FeedItemCount),
                CancellationToken = cancellationToken,
            },
            PostQueries.Summary);

        if (summaries.Count == 0)
        {
            return Result<IReadOnlyList<FeedItem>>.Success([]);
        }

        // The bodies are fetched separately so the summary shape is defined once, in PostQueries.
        var ids = summaries.Select(post => post.Id).ToList();
        var bodies = await posts.FindAsync(
            new SearchOptions<BlogPost>
            {
                Query = visible.And(post => ids.Contains(post.Id)),
                CancellationToken = cancellationToken,
            },
            post => new { post.Id, post.ContentHtml });

        var bodyById = bodies.ToDictionary(item => item.Id, item => item.ContentHtml);

        IReadOnlyList<FeedItem> items = [.. summaries.Select(post => new FeedItem(post, bodyById.GetValueOrDefault(post.Id, string.Empty)))];
        return Result<IReadOnlyList<FeedItem>>.Success(items);
    }

    public async Task<Result<IReadOnlyList<SitemapEntry>>> GetSitemapEntriesAsync(CancellationToken cancellationToken = default)
    {
        var visiblePosts = await posts.FindAsync(
            new SearchOptions<BlogPost>
            {
                Query = PostQueries.Visible(tenants.GetTenantId(), Now()),
                OrderBy = PostQueries.Newest,
                CancellationToken = cancellationToken,
            },
            post => new { post.Slug, post.UpdatedAt });

        var entries = new List<SitemapEntry>
        {
            new(urls.Index(), visiblePosts.Count == 0 ? null : visiblePosts.Max(post => post.UpdatedAt)),
        };

        entries.AddRange(visiblePosts.Select(post => new SitemapEntry(urls.Post(post.Slug), post.UpdatedAt)));

        var usedCategories = await GetCategoriesAsync(cancellationToken);
        if (usedCategories.IsSuccess)
        {
            entries.AddRange(usedCategories.Value
                .Where(item => item.PostCount > 0)
                .Select(item => new SitemapEntry(urls.Category(item.Slug), null)));
        }

        var usedTags = await GetTagsAsync(cancellationToken);
        if (usedTags.IsSuccess)
        {
            entries.AddRange(usedTags.Value.Select(item => new SitemapEntry(urls.Tag(item.Slug), null)));
        }

        return Result<IReadOnlyList<SitemapEntry>>.Success(entries);
    }

    /// <summary>The visible posts, narrowed to a tag or category when a slug is given. NotFound for an unknown slug.</summary>
    private async Task<Result<(Expression<Func<BlogPost, bool>> Filter, TagDto? Tag, CategoryDto? Category)>> ResolveScopeAsync(
        string? tagSlug,
        string? categorySlug,
        CancellationToken cancellationToken)
    {
        string tenantId = tenants.GetTenantId();
        var filter = PostQueries.Visible(tenantId, Now());

        TagDto? tag = null;
        if (!string.IsNullOrWhiteSpace(tagSlug))
        {
            string normalized = tagSlug.Trim().ToLowerInvariant();
            var foundTag = await tags.FindOneAsync(new SearchOptions<BlogTag>
            {
                Query = item => item.TenantId == tenantId && item.Slug == normalized,
                CancellationToken = cancellationToken,
            });

            if (foundTag is null)
            {
                return Result<(Expression<Func<BlogPost, bool>>, TagDto?, CategoryDto?)>.NotFound($"There is no tag '{normalized}'.");
            }

            int tagId = foundTag.Id;
            filter = filter.And(post => post.Tags.Any(link => link.TagId == tagId));
            tag = new TagDto(foundTag.Id, foundTag.Name, foundTag.Slug, 0);
        }

        CategoryDto? category = null;
        if (!string.IsNullOrWhiteSpace(categorySlug))
        {
            string normalized = categorySlug.Trim().ToLowerInvariant();
            var foundCategory = await categories.FindOneAsync(new SearchOptions<BlogCategory>
            {
                Query = item => item.TenantId == tenantId && item.Slug == normalized,
                CancellationToken = cancellationToken,
            });

            if (foundCategory is null)
            {
                return Result<(Expression<Func<BlogPost, bool>>, TagDto?, CategoryDto?)>.NotFound($"There is no category '{normalized}'.");
            }

            int categoryId = foundCategory.Id;
            filter = filter.And(post => post.CategoryId == categoryId);
            category = new CategoryDto(
                foundCategory.Id, foundCategory.Name, foundCategory.Slug, foundCategory.Description, foundCategory.SortOrder, 0);
        }

        return Result<(Expression<Func<BlogPost, bool>>, TagDto?, CategoryDto?)>.Success((filter, tag, category));
    }

    /// <summary>
    /// Posts sharing the most tags with this one, newest first among equals. Falls back to the same
    /// category when the post has no tags.
    /// </summary>
    private async Task<IReadOnlyList<PostSummaryDto>> FindRelatedAsync(
        BlogPost post,
        Expression<Func<BlogPost, bool>> visible,
        CancellationToken cancellationToken)
    {
        int count = options.Value.RelatedPostCount;
        if (count <= 0)
        {
            return [];
        }

        int postId = post.Id;
        var tagIds = post.Tags.Select(link => link.TagId).ToList();

        SearchOptions<BlogPost> search;
        if (tagIds.Count > 0)
        {
            search = new SearchOptions<BlogPost>
            {
                Query = visible.And(item => item.Id != postId && item.Tags.Any(link => tagIds.Contains(link.TagId))),
                OrderBy = query => query
                    .OrderByDescending(item => item.Tags.Count(link => tagIds.Contains(link.TagId)))
                    .ThenByDescending(item => item.PublishedAt),
            };
        }
        else if (post.CategoryId is int categoryId)
        {
            search = new SearchOptions<BlogPost>
            {
                Query = visible.And(item => item.Id != postId && item.CategoryId == categoryId),
                OrderBy = PostQueries.Newest,
            };
        }
        else
        {
            return [];
        }

        search.PageNumber = 1;
        search.PageSize = count;
        search.CancellationToken = cancellationToken;

        return [.. await posts.FindAsync(search, PostQueries.Summary)];
    }

    private async Task<string?> FindRedirectAsync(
        string tenantId,
        string slug,
        Expression<Func<BlogPost, bool>> filter,
        CancellationToken cancellationToken)
    {
        var redirect = await redirects.FindOneAsync(new SearchOptions<BlogSlugRedirect>
        {
            Query = item => item.TenantId == tenantId && item.Slug == slug,
            CancellationToken = cancellationToken,
        });

        if (redirect is null)
        {
            return null;
        }

        int postId = redirect.PostId;
        return await posts.FindOneAsync(
            new SearchOptions<BlogPost>
            {
                Query = filter.And(item => item.Id == postId),
                CancellationToken = cancellationToken,
            },
            item => item.Slug);
    }

    private DateTime Now() => time.GetUtcNow().UtcDateTime;
}