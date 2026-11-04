using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace BlogKrafter.Demo.Infrastructure;

/// <summary>
/// Signs every local request in as an editor so the demo needs no account. Requests from another
/// machine stay anonymous: this must never make a reachable site editable.
/// </summary>
public sealed class DemoAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Demo";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var remote = Context.Connection.RemoteIpAddress;
        if (remote is null || !IPAddress.IsLoopback(remote))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "demo-editor"),
                new Claim(ClaimTypes.Name, "Demo Editor"),
                new Claim(ClaimTypes.Role, "Administrator"),
            ],
            SchemeName);

        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
    }
}