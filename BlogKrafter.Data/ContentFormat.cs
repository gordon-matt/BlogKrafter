namespace BlogKrafter;

/// <summary>How a post's source is written. Stored as an integer, so do not renumber.</summary>
public enum ContentFormat
{
    /// <summary>Markdown, including any HTML the author left in it. The value is 0.</summary>
    Markdown = 0,

    /// <summary>HTML from the host's rich text editor.</summary>
    Html = 1,
}