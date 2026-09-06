using FluentResults;

namespace Moonlight.ApiSdk.Features.Activity;

public interface IActivityService
{
    public Task<Result<Activity>> CreateAsync(string title, string content, string type, int? userId = null,
        Dictionary<string, object>? parameters = null);
}