namespace BlogKrafter.Models;

/// <summary>A post as a feed carries it: the summary plus the full body.</summary>
public sealed record FeedItem(PostSummaryDto Post, string ContentHtml);
