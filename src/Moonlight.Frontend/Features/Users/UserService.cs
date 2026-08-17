using FluentResults;
using Moonlight.Frontend.Infrastructure.Helpers;
using Moonlight.FrontendSdk.Features.Misc;
using Moonlight.Shared.Features.Users;

namespace Moonlight.Frontend.Features.Users;

public class UserService
{
    private readonly IHttpClientFactory  _httpClientFactory;

    public UserService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Result<RangedData<UserDto>>> GetAsync(int limit, int offset)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.GetAsync($"api/admin/users?limit={limit}&offset={offset}");

        var responseResult = await response.GetStatusResultAsync();

        if (responseResult.IsFailed)
            return Result.Fail(responseResult.Errors);

        var model = await response.Content.ReadFromJsonAsync<RangedData<UserDto>>(DtoSerializer.Default.Options);

        if (model is null)
            return Result.Fail("Failed to deserialize model");
        
        return model;
    }
}