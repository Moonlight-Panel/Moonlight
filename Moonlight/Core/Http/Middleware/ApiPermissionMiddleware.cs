using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using MoonCore.Helpers;
using Moonlight.Core.Attributes;

namespace Moonlight.Core.Http.Middleware;

public class ApiPermissionMiddleware
{
    private RequestDelegate Next;

    public ApiPermissionMiddleware(RequestDelegate next)
    {
        Next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (CheckRequest(context))
            await Next(context);
        else
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Permission denied");
        }
    }

    private bool CheckRequest(HttpContext context)
    {
        var endpoint = context.GetEndpoint();

        if (endpoint == null)
            return true;
        
        var metadata = endpoint
            .Metadata
            .GetMetadata<ControllerActionDescriptor>();

        if (metadata == null)
            return true;

        if (metadata.ControllerTypeInfo.CustomAttributes
            .All(x => x.AttributeType != typeof(ApiControllerAttribute)))
            return true;

        var permissionAttr =
            metadata.ControllerTypeInfo.CustomAttributes.FirstOrDefault(x =>
                x.AttributeType == typeof(ApiPermissionAttribute));

        if (permissionAttr == null)
            return true;
        
        if(metadata.)
    }
}