using System.Security.Claims;
using Heroicsoft.NetForge.Abstractions.Auth;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlogKrafter.Infrastructure;

/// <summary>
/// Used only when the host has not registered an <see cref="IUserContextService"/>. Reads the request
/// during prerendering and the circuit's authentication state afterwards, because an interactive
/// circuit has no <c>HttpContext</c>.
/// </summary>
internal sealed class ClaimsUserContextService(
    IHttpContextAccessor httpContextAccessor,
    IServiceProvider services) : IUserContextService
{
    private const string AdministratorRole = "Administrator";

    private static readonly string[] RoleClaimTypes = [ClaimTypes.Role, "role", "roles"];

    public string? GetCurrentUserId()
    {
        var user = Principal();
        return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user?.FindFirst("sub")?.Value
            ?? user?.Identity?.Name;
    }

    public string? GetCurrentUserName()
    {
        var user = Principal();
        return user?.Identity?.Name ?? user?.FindFirst("name")?.Value;
    }

    public bool IsAuthenticated() => Principal() is not null;

    public bool IsAdministrator() =>
        GetRoleNames().Contains(AdministratorRole, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<string> GetRoleNames()
    {
        var user = Principal();
        if (user is null)
        {
            return [];
        }

        var types = new HashSet<string>(RoleClaimTypes, StringComparer.Ordinal);
        if (user.Identity is ClaimsIdentity identity)
        {
            types.Add(identity.RoleClaimType);
        }

        return [.. user.Claims
            .Where(claim => types.Contains(claim.Type))
            .Select(claim => claim.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)];
    }

    /// <summary>The signed-in user, or null for an anonymous caller.</summary>
    private ClaimsPrincipal? Principal()
    {
        var httpUser = httpContextAccessor.HttpContext?.User;
        if (httpUser?.Identity?.IsAuthenticated == true)
        {
            return httpUser;
        }

        if (services.GetService<AuthenticationStateProvider>() is not { } stateProvider)
        {
            return null;
        }

        try
        {
            // The services are synchronous, and on an established circuit this task has already completed.
            var state = Task.Run(stateProvider.GetAuthenticationStateAsync).GetAwaiter().GetResult();
            return state.User.Identity?.IsAuthenticated == true ? state.User : null;
        }
        catch (InvalidOperationException)
        {
            // Outside a circuit the server provider has no state to give.
            return null;
        }
    }
}
