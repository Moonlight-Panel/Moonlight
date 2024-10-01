using System.Security.Claims;
using Moonlight.ApiServer.Database.Entities;

namespace Moonlight.ApiServer.Helpers.Authentication;

public class PermClaimsPrinciple : ClaimsPrincipal
{
    public string[] Permissions { get; private set; }
    public User? CurrentModelNullable { get; private set; }
    public User CurrentModel => CurrentModelNullable!;
    
    public PermClaimsPrinciple(string[] permissions, User? currentModelNullable)
    {
        Permissions = permissions;
        CurrentModelNullable = currentModelNullable;
    }

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