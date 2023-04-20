using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace SensorServer.Helpers;

public class SimpleTokenAuthenticationOptions : AuthenticationSchemeOptions
{
    public IReadOnlySet<string> AllowedTokens { get; set; } = new HashSet<string>();
}

public class SimpleTokenAuthenticationHandler: AuthenticationHandler<SimpleTokenAuthenticationOptions>
{
    public SimpleTokenAuthenticationHandler(IOptionsMonitor<SimpleTokenAuthenticationOptions> options,
        ILoggerFactory logger, 
        UrlEncoder encoder,
        ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Authorization: Bearer sdlfihjslkdfgih
        if (!Request.Headers.Authorization.Any()) return Task.FromResult(AuthenticateResult.Fail("Unauthorized"));
        var authHeader = Request.Headers.Authorization[0];
        if (authHeader == null || !authHeader.ToLower().StartsWith("bearer "))
            return Task.FromResult(AuthenticateResult.Fail("Unauthorized"));
        var token = authHeader["bearer".Length..].Trim();
        if (!Options.AllowedTokens.Contains(token))
            return Task.FromResult(AuthenticateResult.Fail("Unauthorized"));
        var identity = new ClaimsIdentity(new List<Claim>
        {
            new(ClaimTypes.Name, token),
        }, Scheme.Name);
        var principal = new System.Security.Principal.GenericPrincipal(identity, null);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}