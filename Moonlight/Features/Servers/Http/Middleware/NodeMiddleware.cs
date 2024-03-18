using MoonCore.Abstractions;
using Moonlight.Features.Servers.Entities;

namespace Moonlight.Features.Servers.Http.Middleware;

public class NodeMiddleware
{
    private RequestDelegate Next;
    private readonly IServiceProvider ServiceProvider;

    public NodeMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
    {
        Next = next;
        ServiceProvider = serviceProvider;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!context.Request.Path.Value!.StartsWith("/api/servers"))
        {
            await Next(context);
            return;
        }
        
        // Now we actually want to validate the request
        // so every return after this text will prevent
        // the call of the controller action

        // Check if header exists
        if (!context.Request.Headers.ContainsKey("Authorization"))
        {
            // TODO: Add a proper extensions pack to support proper error messages
            context.Response.StatusCode = 403;
            return;
        }

        var token = context.Request.Headers["Authorization"].ToString();

        // Check if header is null
        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = 403;
            return;
        }

        using var scope = ServiceProvider.CreateScope();
        var nodeRepo = scope.ServiceProvider.GetRequiredService<Repository<ServerNode>>();
        
        // Check if any node has the token specified by the request
        var node = nodeRepo
            .Get()
            .FirstOrDefault(x => x.Token == token);

        if (node == null)
        {
            context.Response.StatusCode = 403;
            return;
        }
        
        // Request is valid, because we found a node by this token
        // so now we want to save it for the controller to use and
        // continue in the request pipeline

        context.Items["Node"] = node;
        
        await Next(context);
    }
}