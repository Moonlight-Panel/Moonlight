namespace Moonlight.ApiServer.Http.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate Next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        Next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        
    }
}