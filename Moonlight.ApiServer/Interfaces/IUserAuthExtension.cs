using System.Security.Claims;
using Moonlight.ApiServer.Database.Entities;

namespace Moonlight.ApiServer.Interfaces;

public interface IUserAuthExtension
{
    /// <summary>
    /// This function is called on every sign-in. It should be used to synchronize additional user data from the principal
    /// or extend the claims saved in the user session
    /// </summary>
    /// <param name="user">The current user this method is called for</param>
    /// <param name="principal">The principal after being processed by moonlight itself</param>
    /// <returns>The result of the synchronisation. Returning false will immediately invalidate the sign-in and no other extensions will be called</returns>
    public Task<bool> Sync(User user, ClaimsPrincipal principal);

    /// <summary>
    /// IMPORTANT: Please note that heavy operations should not occur in this method as it will be called for every request
    /// of every user
    /// </summary>
    /// <param name="user">The current user this method is called for</param>
    /// <param name="principal">The principal after being processed by moonlight itself</param>
    /// <returns>The result of the validation. Returning false will immediately invalidate the users session and no other extensions will be called</returns>
    public Task<bool> Validate(User user, ClaimsPrincipal principal);
}