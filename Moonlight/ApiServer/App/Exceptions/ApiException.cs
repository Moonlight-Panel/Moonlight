namespace Moonlight.ApiServer.App.Exceptions;

public class ApiException : Exception
{
    public string? Title { get; set; }
    public string? Detail { get; set; }
    public int? StatusCode { get; set; }
    
    public ApiException(string? title = null, string? detail = null, int? statusCode = null)
    {
        Title = title;
        Detail = detail;
        StatusCode = statusCode;
    }

    public ApiException(string message, string? title, string? detail, int? statusCode) : base(message)
    {
        Title = title;
        Detail = detail;
        StatusCode = statusCode;
    }

    public ApiException(string message, Exception inner, string? title, string? detail, int? statusCode) : base(message, inner)
    {
        Title = title;
        Detail = detail;
        StatusCode = statusCode;
    }
}