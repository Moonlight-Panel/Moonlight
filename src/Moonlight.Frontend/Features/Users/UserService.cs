using FluentResults;
using Moonlight.Frontend.Infrastructure.Helpers;
using Moonlight.FrontendSdk.Features.Misc;
using Moonlight.Shared.Features.Users;

namespace Moonlight.Frontend.Features.Users;

public class UserService
{
    private readonly HttpClient _httpClient;

    public UserService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("Api");
    }

    public async Task<Result<RangedData<UserDto>>> GetAsync(int limit, int offset, string? search = null)
    {
        var query = $"api/admin/users?limit={limit}&offset={offset}";
        
        if(!string.IsNullOrWhiteSpace(search))
            query += $"&search={Uri.EscapeDataString(search)}";
        
        var response = await _httpClient.GetAsync(query);

        var responseResult = await response.GetStatusResultAsync();

        if (responseResult.IsFailed)
            return Result.Fail(responseResult.Errors);

        var model = await response.Content.ReadFromJsonAsync<RangedData<UserDto>>(DtoSerializer.Default.Options);

        if (model is null)
            return Result.Fail("Failed to deserialize model");
        
        return model;
    }

    public async Task<Result<UserDto>> CreateAsync(CreateUserDto requestDto)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/admin/users");
        request.Content = JsonContent.Create(requestDto, DtoSerializer.Default.CreateUserDto);

        using var response = await _httpClient.SendAsync(request);

        var responseResult = await response.GetStatusResultAsync();

        if (responseResult.IsFailed)
            return Result.Fail(responseResult.Errors);

        var model = await response.Content.ReadFromJsonAsync<UserDto>(DtoSerializer.Default.Options);
        
        if(model is null)
            return Result.Fail("Failed to deserialize model");
        
        return model;
    }
    
    public async Task<Result<UserDto>> UpdateAsync(int id, UpdateUserDto requestDto)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"api/admin/users/{id}");
        request.Content = JsonContent.Create(requestDto, DtoSerializer.Default.UpdateUserDto);

        using var response = await _httpClient.SendAsync(request);

        var responseResult = await response.GetStatusResultAsync();

        if (responseResult.IsFailed)
            return Result.Fail(responseResult.Errors);

        var model = await response.Content.ReadFromJsonAsync<UserDto>(DtoSerializer.Default.Options);
        
        if(model is null)
            return Result.Fail("Failed to deserialize model");
        
        return model;
    }

    public async Task<Result> DeleteAsync(int id)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/admin/users/{id}");

        using var response = await _httpClient.SendAsync(request);

        return await response.GetStatusResultAsync();
    }
}