namespace BlogKrafter.Models;

/// <summary>
/// Represents the paged list.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
/// <param name="Items">The items.</param>
/// <param name="Page">The page.</param>
/// <param name="PageSize">The page size.</param>
/// <param name="TotalCount">The total count.</param>
public sealed record PagedList<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    /// <summary>
    /// Gets the page count.
    /// </summary>
    public int PageCount => PageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));

    /// <summary>
    /// Gets a value indicating whether this instance has previous.
    /// </summary>
    public bool HasPrevious => Page > 1;

    /// <summary>
    /// Gets a value indicating whether this instance has next.
    /// </summary>
    public bool HasNext => Page < PageCount;
}