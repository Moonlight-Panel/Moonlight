using Moonlight.ApiServer.App.Database.Entities;

namespace Moonlight.ApiServer.App.Extensions;

public static class HttpContextExtensions
{
    public static void SetPermissions(this HttpContext context, string[] permissions)
    {
        context.Items["Permissions"] = permissions;
    }

    public static string[] GetPermissions(this HttpContext context)
    {
        if (!context.Items.TryGetValue("Permissions", out var permissions))
            return [];

        return permissions as string[] ?? [];
    }

    public static void SetCurrentUser(this HttpContext context, User user)
    {
        context.Items["CurrentUser"] = user;
    }

    public static User GetCurrentUser(this HttpContext context) => GetCurrentUserNullable(context)!;
    
    public static User? GetCurrentUserNullable(this HttpContext context)
    {
        if (context.Items.TryGetValue("CurrentUser", out var currentUser))
            return currentUser as User;

        return null;
    }
    
    public static bool HasPermission(this HttpContext context, string requiredPermission)
    {
        var permissions = context.GetPermissions();

        // Check for wildcard permission
        if (permissions.Contains("*"))
        {
            return true; // User has all permissions
        }
    
        var requiredSegments = requiredPermission.Split('.');

        // Check if the user has the exact permission or a wildcard match
        foreach (var permission in permissions)
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

    public static bool HasPermissions(this HttpContext context, string[] permissions) =>
        permissions.All(permission => context.HasPermission(permission));
}