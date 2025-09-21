using System.Security.Claims;
using Moonlight.Shared.Http.Responses.Auth;

namespace Moonlight.ApiServer.Interfaces;

public interface IAuthCheckExtension
{
    /// <summary>
    /// This function will be called by the frontend reaching out to the api server for claim information.
    /// You can use this function to give your frontend plugins access to user specific data which is
    /// static for the session. E.g. the avatar url of a user
    /// </summary>
    /// <param name="principal">The principal of the current signed-in user</param>
    /// <returns>An array of claim responses which gets added to the list of claims to send to the frontend</returns>
    public Task<AuthClaimResponse[]> GetFrontendClaimsAsync(ClaimsPrincipal principal);
}