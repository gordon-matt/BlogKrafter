namespace BlogKrafter.Tenancy;

/// <summary>
/// Separates the posts, categories and tags of several sites that share one database. A multi-site
/// host registers its own implementation (scoped is fine) that returns the current site's key.
/// </summary>
public interface IBlogTenantProvider
{
    /// <summary>The current site. Empty for a single-site host. Values longer than 64 characters are rejected.</summary>
    string GetTenantId();
}

/// <summary>The default for a single-site host: everything belongs to one blog.</summary>
public sealed class SingleTenantProvider : IBlogTenantProvider
{
    public string GetTenantId() => string.Empty;
}