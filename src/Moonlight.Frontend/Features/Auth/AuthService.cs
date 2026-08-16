using FluentResults;
using Moonlight.Frontend.Infrastructure.Helpers;
using Moonlight.Shared.Features.Auth;

namespace Moonlight.Frontend.Features.Auth;

public class AuthService
{
    private readonly IHttpClientFactory  _httpClientFactory;

    public AuthService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Result<InfoDto>> GetAsync()
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.GetAsync("api/auth/info");

        var responseResult = await response.GetStatusResultAsync();

        if (responseResult.IsFailed)
            return Result.Fail(responseResult.Errors);
        
        var model = await response.Content.ReadFromJsonAsync<InfoDto>();
        
        if(model is null)
            return Result.Fail("Failed to deserialize model");
        
        return model;
    }
}