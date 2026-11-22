namespace BlogKrafter;

/// <summary>
/// Stored as an integer, so do not renumber. A published post with a future publish date is
/// scheduled: it stays hidden until that moment without anything having to flip it.
/// </summary>
public enum PostStatus
{
    /// <summary>Not shown to readers. The value is 0.</summary>
    Draft = 0,

    /// <summary>Live once <c>PublishedAt</c> has arrived. A future date stays hidden until then. The value is 1.</summary>
    Published = 1,
}