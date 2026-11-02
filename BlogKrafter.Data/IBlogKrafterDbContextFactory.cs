namespace BlogKrafter.Data;

/// <summary>
/// BlogKrafter's own context factory. It extends rather than replaces Extenso's
/// <see cref="IDbContextFactory"/>: a host built on Extenso registers that interface for its own
/// context, and BlogKrafter must not take it over. Its repositories are built against this type instead.
/// </summary>
public interface IBlogKrafterDbContextFactory : IDbContextFactory
{
    /// <summary>One of <see cref="Constants.DatabaseProviders"/>.</summary>
    string ProviderName { get; }
}
