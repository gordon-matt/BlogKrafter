namespace BlogKrafter;

/// <summary>
/// Stored as an integer, so do not renumber. A published post with a future publish date is
/// scheduled: it stays hidden until that moment without anything having to flip it.
/// </summary>
public enum PostStatus
{
    /// <summary>
    /// The draft value.
    /// </summary>
    Draft = 0,
    /// <summary>
    /// The published value.
    /// </summary>
    Published = 1,
}