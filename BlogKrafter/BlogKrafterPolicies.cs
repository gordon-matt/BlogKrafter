namespace BlogKrafter;

/// <summary>
/// Provides blog krafter policies.
/// </summary>
public static class BlogKrafterPolicies
{
    /// <summary>
    /// Guards the editor screens. Built from <see cref="BlogKrafterOptions.ManagerRoles"/> unless the
    /// host registers its own policy with this name.
    /// </summary>
    public const string Manage = "BlogKrafter.Manage";
}