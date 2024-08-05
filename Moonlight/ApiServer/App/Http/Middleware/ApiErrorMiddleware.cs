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