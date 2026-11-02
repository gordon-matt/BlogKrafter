namespace BlogKrafter.Taxonomy;

public sealed class BlogCategoryService(
    ILogger<BlogCategoryService> logger,
    IUserContextService userContext,
    IBlogTenantProvider tenants,
    IRepository<BlogCategory> categories,
    IRepository<BlogPost> posts) : IBlogCategoryService
{
    public async Task<Result<IReadOnlyList<CategoryDto>>> ListAsync(CancellationToken cancellationToken = default)
    {
        if (userContext.GetCurrentUserId() is null)
        {
            return Result<IReadOnlyList<CategoryDto>>.Forbidden();
        }

        string tenantId = tenants.GetTenantId();

        var counts = await posts.FindGroupedAsync(
            new SearchOptions<BlogPost>
            {
                Query = post => post.TenantId == tenantId && post.CategoryId != null,
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

        IReadOnlyList<CategoryDto> result = [.. all.Select(item => ToDto(item, countById.GetValueOrDefault(item.Id)))];
        return Result<IReadOnlyList<CategoryDto>>.Success(result);
    }

    public async Task<Result<CategoryDto>> SaveAsync(SaveCategoryRequest request, CancellationToken cancellationToken = default)
    {
        if (userContext.GetCurrentUserId() is not string userId)
        {
            return Result<CategoryDto>.Forbidden();
        }

        string tenantId = tenants.GetTenantId();
        var errors = new List<ValidationError>();

        string name = request.Name?.Trim() ?? string.Empty;
        if (name.Length == 0)
        {
            errors.Add(new ValidationError(nameof(request.Name), "Give the category a name."));
        }
        else if (name.Length > Constants.MaxLengths.CategoryName)
        {
            errors.Add(new ValidationError(nameof(request.Name), $"Names can be at most {Constants.MaxLengths.CategoryName} characters."));
        }

        string slug = Slugs.Generate(string.IsNullOrWhiteSpace(request.Slug) ? name : request.Slug);
        if (name.Length > 0 && slug.Length == 0)
        {
            errors.Add(new ValidationError(nameof(request.Slug), "Use letters or numbers in the address."));
        }

        string? description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        if (description?.Length > Constants.MaxLengths.CategoryDescription)
        {
            errors.Add(new ValidationError(
                nameof(request.Description),
                $"Descriptions can be at most {Constants.MaxLengths.CategoryDescription} characters."));
        }

        if (errors.Count > 0)
        {
            return Result<CategoryDto>.Invalid(errors);
        }

        int self = request.Id ?? 0;
        if (await categories.ExistsAsync(
            item => item.TenantId == tenantId && item.Slug == slug && item.Id != self,
            ContextOptions.ForCancellationToken(cancellationToken)))
        {
            return Result<CategoryDto>.Invalid(new ValidationError(nameof(request.Slug), "Another category already uses this address."));
        }

        var context = ContextOptions.ForCancellationToken(cancellationToken);
        if (request.Id is null)
        {
            var created = await categories.InsertAsync(
                new BlogCategory
                {
                    TenantId = tenantId,
                    Name = name,
                    Slug = slug,
                    Description = description,
                    SortOrder = request.SortOrder,
                },
                context);

            logger.LogInformation("User {UserId} created blog category {CategoryId}", userId, created.Id);
            return Result<CategoryDto>.Success(ToDto(created, 0));
        }

        var existing = await categories.FindOneAsync(new SearchOptions<BlogCategory>
        {
            Query = item => item.Id == self && item.TenantId == tenantId,
            CancellationToken = cancellationToken,
        });

        if (existing is null)
        {
            return Result<CategoryDto>.NotFound($"Category {self} was not found.");
        }

        existing.Name = name;
        existing.Slug = slug;
        existing.Description = description;
        existing.SortOrder = request.SortOrder;

        var updated = await categories.UpdateAsync(existing, context);
        int postCount = await posts.CountAsync(post => post.CategoryId == updated.Id, context);

        logger.LogInformation("User {UserId} updated blog category {CategoryId}", userId, updated.Id);
        return Result<CategoryDto>.Success(ToDto(updated, postCount));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (userContext.GetCurrentUserId() is not string userId)
        {
            return Result.Forbidden();
        }

        string tenantId = tenants.GetTenantId();
        var context = ContextOptions.ForCancellationToken(cancellationToken);

        if (!await categories.ExistsAsync(item => item.Id == id && item.TenantId == tenantId, context))
        {
            return Result.NotFound($"Category {id} was not found.");
        }

        // The foreign key sets the posts' category to null; the delete is set-based, so nothing is loaded.
        await categories.DeleteAsync(item => item.Id == id && item.TenantId == tenantId, context);

        logger.LogInformation("User {UserId} deleted blog category {CategoryId}", userId, id);
        return Result.Success();
    }

    private static CategoryDto ToDto(BlogCategory category, int postCount) =>
        new(category.Id, category.Name, category.Slug, category.Description, category.SortOrder, postCount);
}
