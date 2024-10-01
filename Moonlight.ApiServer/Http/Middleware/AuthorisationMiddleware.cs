using Microsoft.AspNetCore.Mvc.Controllers;
using Moonlight.ApiServer.Attributes;

namespace Moonlight.ApiServer.Http.Middleware;

public class AuthorisationMiddleware
{
    private readonly RequestDelegate Next;
    private readonly ILogger<AuthorisationMiddleware> Logger;

    public AuthorisationMiddleware(RequestDelegate next, ILogger<AuthorisationMiddleware> logger)
    {
        Next = next;
        Logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await Next(context);
    }

    private async Task Authorize(HttpContext context)
    {
        
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