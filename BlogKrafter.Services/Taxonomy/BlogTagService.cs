namespace BlogKrafter.Taxonomy;

/// <summary>
/// Represents the blog tag service.
/// </summary>
public sealed class BlogTagService(
    ILogger<BlogTagService> logger,
    IUserContextService userContext,
    IBlogTenantProvider tenants,
    IRepository<BlogTag> tags,
    IRepository<BlogPostTag> postTags) : IBlogTagService
{
    private const int SuggestionCount = 10;

    /// <summary>
    /// Lists the value.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the outcome for the tags.</returns>
    public async Task<Result<IReadOnlyList<TagDto>>> ListAsync(CancellationToken cancellationToken = default)
    {
        if (userContext.GetCurrentUserId() is null)
        {
            return Result<IReadOnlyList<TagDto>>.Forbidden();
        }

        string tenantId = tenants.GetTenantId();
        var counts = await CountPostsAsync(tenantId, cancellationToken);

        var all = await tags.FindAsync(new SearchOptions<BlogTag>
        {
            Query = tag => tag.TenantId == tenantId,
            OrderBy = query => query.OrderBy(tag => tag.Name),
            CancellationToken = cancellationToken,
        });

        IReadOnlyList<TagDto> result = [.. all.Select(tag => new TagDto(tag.Id, tag.Name, tag.Slug, counts.GetValueOrDefault(tag.Id)))];
        return Result<IReadOnlyList<TagDto>>.Success(result);
    }

    /// <summary>
    /// Searches the value.
    /// </summary>
    /// <param name="term">The term.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the outcome for the tags.</returns>
    public async Task<Result<IReadOnlyList<TagDto>>> SearchAsync(string? term, CancellationToken cancellationToken = default)
    {
        if (userContext.GetCurrentUserId() is null)
        {
            return Result<IReadOnlyList<TagDto>>.Forbidden();
        }

        string tenantId = tenants.GetTenantId();
        string lowered = term?.Trim().ToLowerInvariant() ?? string.Empty;

        var found = await tags.FindAsync(new SearchOptions<BlogTag>
        {
            Query = tag => tag.TenantId == tenantId && (lowered.Length == 0 || tag.Name.ToLower().Contains(lowered)),
            OrderBy = query => query.OrderBy(tag => !tag.Name.ToLower().StartsWith(lowered)).ThenBy(tag => tag.Name),
            PageNumber = 1,
            PageSize = SuggestionCount,
            CancellationToken = cancellationToken,
        });

        IReadOnlyList<TagDto> result = [.. found.Select(tag => new TagDto(tag.Id, tag.Name, tag.Slug, 0))];
        return Result<IReadOnlyList<TagDto>>.Success(result);
    }

    /// <summary>
    /// Renames the value.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="name">The name.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the outcome for the tag.</returns>
    public async Task<Result<TagDto>> RenameAsync(int id, string name, CancellationToken cancellationToken = default)
    {
        if (userContext.GetCurrentUserId() is not string userId)
        {
            return Result<TagDto>.Forbidden();
        }

        string trimmed = name?.Trim() ?? string.Empty;
        string slug = Slugs.Generate(trimmed);
        if (slug.Length == 0)
        {
            return Result<TagDto>.Invalid(new ValidationError(nameof(name), "Use letters or numbers in the tag."));
        }

        if (trimmed.Length > Constants.MaxLengths.TagName)
        {
            return Result<TagDto>.Invalid(new ValidationError(nameof(name), $"Tags can be at most {Constants.MaxLengths.TagName} characters."));
        }

        string tenantId = tenants.GetTenantId();
        var context = ContextOptions.ForCancellationToken(cancellationToken);

        var tag = await tags.FindOneAsync(new SearchOptions<BlogTag>
        {
            Query = item => item.Id == id && item.TenantId == tenantId,
            CancellationToken = cancellationToken,
        });

        if (tag is null)
        {
            return Result<TagDto>.NotFound($"Tag {id} was not found.");
        }

        var target = await tags.FindOneAsync(new SearchOptions<BlogTag>
        {
            Query = item => item.TenantId == tenantId && item.Slug == slug && item.Id != id,
            CancellationToken = cancellationToken,
        });

        if (target is null)
        {
            tag.Name = trimmed;
            tag.Slug = slug;
            var renamed = await tags.UpdateAsync(tag, context);
            int count = await postTags.CountAsync(link => link.TagId == id, context);

            logger.LogInformation("User {UserId} renamed blog tag {TagId}", userId, id);
            return Result<TagDto>.Success(new TagDto(renamed.Id, renamed.Name, renamed.Slug, count));
        }

        int targetId = target.Id;
        var alreadyTagged = await postTags.FindAsync(
            new SearchOptions<BlogPostTag> { Query = link => link.TagId == targetId, CancellationToken = cancellationToken },
            link => link.PostId);

        var alreadyTaggedIds = alreadyTagged.ToList();
        var moving = await postTags.FindAsync(
            new SearchOptions<BlogPostTag>
            {
                Query = link => link.TagId == id && !alreadyTaggedIds.Contains(link.PostId),
                CancellationToken = cancellationToken,
            },
            link => link.PostId);

        if (moving.Count > 0)
        {
            await postTags.InsertAsync([.. moving.Select(postId => new BlogPostTag { PostId = postId, TagId = targetId })], context);
        }

        await tags.DeleteAsync(item => item.Id == id, context);
        int merged = await postTags.CountAsync(link => link.TagId == targetId, context);

        logger.LogInformation("User {UserId} merged blog tag {TagId} into {TargetTagId}", userId, id, targetId);
        return Result<TagDto>.Success(new TagDto(target.Id, target.Name, target.Slug, merged));
    }

    /// <summary>
    /// Deletes the value.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result.</returns>
    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (userContext.GetCurrentUserId() is not string userId)
        {
            return Result.Forbidden();
        }

        string tenantId = tenants.GetTenantId();
        int deleted = await tags.DeleteAsync(
            tag => tag.Id == id && tag.TenantId == tenantId,
            ContextOptions.ForCancellationToken(cancellationToken));

        if (deleted == 0)
        {
            return Result.NotFound($"Tag {id} was not found.");
        }

        logger.LogInformation("User {UserId} deleted blog tag {TagId}", userId, id);
        return Result.Success();
    }

    private async Task<Dictionary<int, int>> CountPostsAsync(string tenantId, CancellationToken cancellationToken)
    {
        var counts = await postTags.FindGroupedAsync(
            new SearchOptions<BlogPostTag>
            {
                Query = link => link.Tag!.TenantId == tenantId,
                CancellationToken = cancellationToken,
            },
            link => link.TagId,
            group => new { TagId = group.Key, Count = group.Count() });

        return counts.ToDictionary(item => item.TagId, item => item.Count);
    }
}