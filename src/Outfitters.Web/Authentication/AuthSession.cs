using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Outfitters.Web.Authentication;

public sealed class AuthSession
{
    public string? AccessToken { get; private set; }
    public ClaimsPrincipal Principal { get; private set; } = new(new ClaimsIdentity());

    public void SignIn(string token)
    {
        AccessToken = token;
        Principal = CreatePrincipal(token);
    }

    public void SignOut()
    {
        AccessToken = null;
        Principal = new ClaimsPrincipal(new ClaimsIdentity());
    }

    private static ClaimsPrincipal CreatePrincipal(string token)
    {
        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var claims = jwt.Claims.ToList();

            var name = claims.FirstOrDefault(x => x.Type is ClaimTypes.Name or "name" or "unique_name");
            if (name is not null && claims.All(x => x.Type != ClaimTypes.Name))
            {
                claims.Add(new Claim(ClaimTypes.Name, name.Value));
            }

            foreach (var role in claims.Where(x => x.Type is "role" or "roles").Select(x => x.Value).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (claims.All(x => x.Type != ClaimTypes.Role || !string.Equals(x.Value, role, StringComparison.OrdinalIgnoreCase)))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer", ClaimTypes.Name, ClaimTypes.Role));
        }
        catch
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }
    }
}

public sealed class OrmsAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly AuthSession _session;

    public OrmsAuthenticationStateProvider(AuthSession session) => _session = session;

    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        Task.FromResult(new AuthenticationState(_session.Principal));

    public void SignIn(string token)
    {
        _session.SignIn(token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void SignOut()
    {
        _session.SignOut();
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
