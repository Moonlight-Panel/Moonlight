using Microsoft.AspNetCore.Mvc.Controllers;
using MoonCore.Abstractions;
using Moonlight.Core.Attributes;
using Moonlight.Core.Database.Entities;
using Newtonsoft.Json;

namespace Moonlight.Core.Http.Middleware;

public class ApiPermissionMiddleware
{
    private RequestDelegate Next;
    private readonly IServiceProvider Provider;

    public ApiPermissionMiddleware(RequestDelegate next, IServiceProvider provider)
    {
        Next = next;
        Provider = provider;
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

        var controllerAttrInfo = metadata.ControllerTypeInfo.CustomAttributes
            .FirstOrDefault(x => x.AttributeType == typeof(ApiPermissionAttribute));

        var methodAttrInfo = metadata.MethodInfo.CustomAttributes
            .FirstOrDefault(x => x.AttributeType == typeof(ApiPermissionAttribute));

        if (methodAttrInfo == null && controllerAttrInfo == null)
            return true;

        if (!context.Request.Headers.TryGetValue("Authorization", out var apiKeySv))
            return false;

        // Entity framework won't work with the StringValues type returned by the Headers.TryGetValue method
        // that's why we convert that to a regular string here
        var apiKey = apiKeySv.ToString();
        
        if (string.IsNullOrEmpty(apiKey))
            return false;

        using var scope = Provider.CreateScope();
        var apiKeyRepo = scope.ServiceProvider.GetRequiredService<Repository<ApiKey>>();

        var apiKeyModel = apiKeyRepo
            .Get()
            .FirstOrDefault(x => x.Key == apiKey);

        if (apiKeyModel == null)
            return false;

        if (apiKeyModel.ExpiresAt < DateTime.UtcNow)
            return false;

        var permissions = JsonConvert.DeserializeObject<string[]>(apiKeyModel.PermissionJson) ?? Array.Empty<string>();

        if (controllerAttrInfo != null)
        {
            var permissionToLookFor = controllerAttrInfo.ConstructorArguments.First().Value as string;

            if (permissionToLookFor != null && !permissions.Contains(permissionToLookFor))
                return false;
        }
        
        if (methodAttrInfo != null)
        {
            var permissionToLookFor = methodAttrInfo.ConstructorArguments.First().Value as string;

            if (permissionToLookFor != null && !permissions.Contains(permissionToLookFor))
                return false;
        }

        return true;
    }
}