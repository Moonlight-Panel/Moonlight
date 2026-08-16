using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Moonlight.Api.Features.Auth;

public class NameClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var defaultIdentity = principal.Identities.FirstOrDefault();

        if (defaultIdentity is null || principal.HasClaim(x => x.Type == ClaimTypes.Name))
            return Task.FromResult(principal);

        var nameClaim = principal.FindFirst(x => x.Type == "name");

        if (nameClaim is null)
            return Task.FromResult(principal);

        defaultIdentity.AddClaim(new Claim(ClaimTypes.Name, nameClaim.Value));

        return Task.FromResult(principal);
    }
}