using MoonCore.Attributes;

namespace Moonlight.Client.App.Services;

[Scoped]
public class IdentityService
{
    public bool IsLoggedIn { get; private set; }
    public string Token { get; private set; } = "";
    public string Username { get; set; }
    public string Email { get; set; }
    public string[] Permissions { get; set; }

    public readonly HttpClient Http;
    
    public IdentityService(HttpClient http)
    {
        Http = http;
    }

    public void SetLoginState(bool newState)
    {
        IsLoggedIn = newState;
    }

    public void SetToken(string token)
    {
        Token = token;

        if (Http.DefaultRequestHeaders.Contains("Authorization"))
            Http.DefaultRequestHeaders.Remove("Authorization");
        
        if(string.IsNullOrEmpty(token))
            return;
        
        Http.DefaultRequestHeaders.Add("Authorization", token);
    }
    
    public bool HasPermission(string requiredPermission)
    {
        // Check for wildcard permission
        if (Permissions.Contains("*"))
        {
            return true; // User has all permissions
        }
    
        var requiredSegments = requiredPermission.Split('.');

        // Check if the user has the exact permission or a wildcard match
        foreach (var permission in Permissions)
        {
            var permissionSegments = permission.Split('.');

            // Iterate over the segments of the required permission
            for (int i = 0; i < requiredSegments.Length; i++)
            {
                // If the current segment matches or is a wildcard, continue to the next segment
                if (i < permissionSegments.Length && requiredSegments[i] == permissionSegments[i] || permissionSegments[i] == "*")
                {
                    // If we've reached the end of the permissionSegments array, it means we've found a match
                    if (i == permissionSegments.Length - 1)
                    {
                        return true; // Found an exact match or a wildcard match
                    }
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