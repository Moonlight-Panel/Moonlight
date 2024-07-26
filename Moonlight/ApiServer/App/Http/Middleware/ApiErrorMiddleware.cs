using Moonlight.ApiServer.App.Exceptions;

namespace Moonlight.ApiServer.App.Http.Middleware;

public class ApiErrorMiddleware
{
    private readonly RequestDelegate Next;

    public ApiErrorMiddleware(RequestDelegate next)
    {
        Next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await Next(context);
        }
        catch (MissingPermissionException e)
        {
            var requiredPermissions = e.RequiredPermissions;
            
            if (requiredPermissions.Length == 1 && requiredPermissions[0] == "meta.authenticated")
            {
                await Results.Problem(
                    title: "You need to be logged in in order to use this endpoint",
                    statusCode: 401
                ).ExecuteAsync(context);

                return;
            }

            await Results.Problem(
                title: "You dont have the required permission",
                detail: string.Join(";", requiredPermissions),
                statusCode: 403
            ).ExecuteAsync(context);
        }
        catch (ApiException e)
        {
            await Results.Problem(
                title: e.Title,
                detail: e.Detail,
                statusCode: e.StatusCode
            ).ExecuteAsync(context);
        }
    }
}