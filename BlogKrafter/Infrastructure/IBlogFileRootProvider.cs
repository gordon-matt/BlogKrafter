namespace BlogKrafter.Infrastructure;

/// <summary>
/// Chooses the folder the file picker and rich text editor open on. The host owns where files live, so
/// a site that keeps each tenant's uploads apart registers an implementation (scoped is fine) that
/// returns the current tenant's folder. The value is passed to Pickstack as the picker's root.
/// </summary>
public interface IBlogFileRootProvider
{
    /// <summary>A folder under the host's upload directory, such as <c>site-3</c>. Null opens the upload directory itself.</summary>
    string? GetFileRoot();
}

/// <summary>The default: the picker opens whatever the host's file manager is configured to show.</summary>
public sealed class DefaultFileRootProvider : IBlogFileRootProvider
{
    public string? GetFileRoot() => null;
}