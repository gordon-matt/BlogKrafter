namespace BlogKrafter.Infrastructure;

/// <summary>The site's absolute root, for feeds and the links search engines and social cards need.</summary>
internal static class SiteAddress
{
    public static string From(BlogKrafterOptions options, HttpRequest request) =>
        options.SiteUrl ?? $"{request.Scheme}://{request.Host}{request.PathBase}";

    public static string From(BlogKrafterOptions options, NavigationManager navigation) =>
        options.SiteUrl ?? navigation.BaseUri.TrimEnd('/');
}
