using FluentResults;

namespace Moonlight.Frontend.Infrastructure.Helpers;

public static class ResultExtensions
{
    public static string GetReasons(this Result result)
        => string.Join(", ", result.Reasons.Select(x => x.Message));
    
    public static string GetReasons<T>(this Result<T> result)
        => string.Join(", ", result.Reasons.Select(x => x.Message));
}