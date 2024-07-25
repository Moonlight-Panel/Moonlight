using Microsoft.AspNetCore.Mvc.Controllers;
using Moonlight.ApiServer.App.Attributes;
using Moonlight.ApiServer.App.Extensions;

namespace Moonlight.ApiServer.App.Http.Middleware;

public class PermissionCheckMiddleware
{
    private readonly RequestDelegate Next;

    public PermissionCheckMiddleware(RequestDelegate next)
    {
        Next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (await Check(context))
            await Next(context);
    }

    private async Task<bool> Check(HttpContext context)
    {
        var requiredPermissions = ResolveRequiredPermissions(context);

        if (requiredPermissions.Length == 0)
            return true;

        if (context.HasPermissions(requiredPermissions))
            return true;

        if (requiredPermissions.Length == 1 && requiredPermissions[0] == "meta.authenticated")
        {
            await Results.Problem(
                title: "You need to be logged in in order to use this endpoint",
                statusCode: 401
            ).ExecuteAsync(context);

            return false;
        }

        await Results.Problem(
            title: "You dont have the required permission",
            detail: string.Join(";", requiredPermissions),
            statusCode: 403
        ).ExecuteAsync(context);

        return false;
    }
    
    private string[] ResolveRequiredPermissions(HttpContext context)
    {
        // Basic handling
        var endpoint = context.GetEndpoint();
        
        if (endpoint == null)
            return [];
        
        var metadata = endpoint
            .Metadata
            .GetMetadata<ControllerActionDescriptor>();

        if (metadata == null)
            return [];

        // Retrieve attribute infos
        var controllerAttrInfo = metadata
            .ControllerTypeInfo
            .CustomAttributes
            .FirstOrDefault(x => x.AttributeType == typeof(RequirePermissionAttribute));

        var methodAttrInfo = metadata
            .MethodInfo
            .CustomAttributes
            .FirstOrDefault(x => x.AttributeType == typeof(RequirePermissionAttribute));

        // Retrieve permissions from attribute infos
        var controllerPermission = controllerAttrInfo != null
            ? controllerAttrInfo.ConstructorArguments.First().Value as string
            : null;
        
        var methodPermission = methodAttrInfo != null
            ? methodAttrInfo.ConstructorArguments.First().Value as string
            : null;

        // If both have a permission flag, return both
        if (controllerPermission != null && methodPermission != null)
            return [controllerPermission, methodPermission];
        
        // If either of them have a permission set, return it
        if (controllerPermission != null)
            return [controllerPermission];
        
        if (methodPermission != null)
            return [methodPermission];
        
        // If both have no permission set, allow everyone to access it
        return [];
    }
}