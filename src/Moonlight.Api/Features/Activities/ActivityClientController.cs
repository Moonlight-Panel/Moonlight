using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moonlight.ApiSdk.Features.Activity;
using Moonlight.Shared.Features.Activity;

namespace Moonlight.Api.Features.Activities;

[Authorize]
[ApiController]
public class ActivityClientController : Controller
{
    private readonly IActivityQueryService _activityQueryService;

    public ActivityClientController(IActivityQueryService activityQueryService)
    {
        _activityQueryService = activityQueryService;
    }

    [HttpGet]
    [Route("/api/client/activities")]
    public async Task<ActionResult<ActivityDto[]>> GetClientActivity([FromQuery] int offset = 0, [FromQuery] int limit = 25)
    {
        var username = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var activities = _activityQueryService.Query()
            .Where(x => x.User.Username == username)
            .OrderBy(x => x.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToArray();

        var dtos = activities.Select(x => new ActivityDto()
        {
            Id = x.Id,
            Title = x.Title,
            Content = x.Content,
            Type = x.Type,
            Parameters = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(x.Parameters) ?? new(),
            CreatedAt = x.CreatedAt
        }).ToArray();

        return dtos;
    }
}