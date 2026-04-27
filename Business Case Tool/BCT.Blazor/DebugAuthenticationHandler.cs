using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace BCT.Blazor;
#pragma warning disable CS0618 // Type or member is obsolete, this is only ever loaded in debug.
internal class DebugAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
#if DEBUG
    public DebugAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)

        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "DebugUser"),
            new Claim(ClaimTypes.Email, "debug@localhost"),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(claims, "DebugScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "DebugScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
#else
    public DebugAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ISystemClock clock)

    : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
{
            new Claim(ClaimTypes.Name, ""),
            new Claim(ClaimTypes.Email, ""),
        };

        var identity = new ClaimsIdentity(claims, "DebugScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "DebugScheme");

        return Task.FromResult(AuthenticateResult.Fail(new Exception("Trying to debug auth in production!")));
    }

#endif
}
#pragma warning restore CS0618 // Type or member is obsolete