using MoonCore.Attributes;
using MoonCore.Blazor.Tailwind.Services;
using MoonCore.Exceptions;
using MoonCore.Helpers;
using Moonlight.Shared.Http.Responses.Auth;

namespace Moonlight.Client.Services;

[Scoped]
public class IdentityService
{
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string[] Permissions { get; private set; }
    public bool IsLoggedIn { get; private set; }

    public event Func<Task> OnStateChanged;

    private readonly CookieService CookieService;
    private readonly HttpApiClient ApiClient;
    
    public IdentityService(CookieService cookieService, HttpApiClient apiClient)
    {
        CookieService = cookieService;
        ApiClient = apiClient;
    }

    #region Login / Logout

    public async Task Check()
    {
        try
        {
            var response = await ApiClient.GetJson<CheckResponse>("api/auth/check");

            Username = response.Username;
            Email = response.Email;
            Permissions = response.Permissions;

            IsLoggedIn = true;
        }
        catch (HttpApiException)
        {
            IsLoggedIn = false;
        }

        //await OnStateChanged?.Invoke();
    }

    public async Task Login(string token)
    {
        await CookieService.SetValue("token", token, 30);
        await Check();
    }

    public async Task Logout()
    {
        await CookieService.SetValue("token", "", 30);
        await Check();
    }

    #endregion
    
    public bool HasPermission(string requiredPermission)
    {
        // Check for wildcard permission
        if (Permissions.Contains("*"))
            return true;

        var requiredSegments = requiredPermission.Split('.');

        // Check if the user has the exact permission or a wildcard match
        foreach (var permission in Permissions)
        {
            var permissionSegments = permission.Split('.');

            // Iterate over the segments of the required permission
            for (var i = 0; i < requiredSegments.Length; i++)
            {
                // If the current segment matches or is a wildcard, continue to the next segment
                if (i < permissionSegments.Length && requiredSegments[i] == permissionSegments[i] ||
                    permissionSegments[i] == "*")
                {
                    // If we've reached the end of the permissionSegments array, it means we've found a match
                    if (i == permissionSegments.Length - 1)
                        return true; // Found an exact match or a wildcard match
                }
                else
                {
                    // If we reach here, it means the segments don't match and we break out of the loop
                    break;
                }
            }
        }

        // No matching permission found
        return false;
    }
}