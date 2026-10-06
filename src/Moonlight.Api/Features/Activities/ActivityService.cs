using System.Text.Json;
using FluentResults;
using Moonlight.Api.Infrastructure.Database;
using Moonlight.ApiSdk.Features.Activity;
using Moonlight.ApiSdk.Features.Users;

namespace Moonlight.Api.Features.Activities;

public class ActivityService : IActivityService
{
    private readonly IUserQueryService _userQueryService;
    private readonly DataContext _dataContext;

    public ActivityService(IUserQueryService userQueryService, DataContext dataContext)
    {
        _userQueryService = userQueryService;
        _dataContext = dataContext;
    }

    public async Task<Result<Activity>> CreateAsync(string title, string content, string type, int? userId = null, Dictionary<string, object>? parameters = null)
    {
        User? user = null;

        if (userId is not null)
        {
            var result = await _userQueryService.FindByIdAsync(userId.Value);

            if (result.IsFailed)
            {
                return Result.Fail(result.Errors);
            }

            user = result.Value;
        }

        var json = JsonSerializer.Serialize(parameters);

        var activity = new Activity()
        {
            Title = title,
            Content = content,
            Type = type,
            User = user,
            Parameters = json,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _dataContext.Activities.AddAsync(activity);

        await _dataContext.SaveChangesAsync();

        return Result.Ok(activity);
    }
}