namespace BlogKrafter.Models;

/// <summary>One page of a list, with enough to build the pager.</summary>
/// <typeparam name="T">The row type.</typeparam>
/// <param name="Items">The rows on this page.</param>
/// <param name="Page">The page number, starting at 1.</param>
/// <param name="PageSize">How many rows a full page holds.</param>
/// <param name="TotalCount">How many rows match, across every page.</param>
public sealed record PagedList<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    /// <summary>How many pages <see cref="TotalCount"/> fills. At least 1, including when the page size is zero or less.</summary>
    public int PageCount => PageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));

    /// <summary><see langword="true"/> when <see cref="PagedList{T}.Page"/> is past the first page.</summary>
    public bool HasPrevious => Page > 1;

    /// <summary><see langword="true"/> when another page follows this one.</summary>
    public bool HasNext => Page < PageCount;
}