using FluentResults;
using Moonlight.Frontend.Infrastructure.Helpers;
using Moonlight.FrontendSdk.Features.Misc;
using Moonlight.Shared.Features.Roles;
using Moonlight.Shared.Features.Users;

namespace Moonlight.Frontend.Features.Roles;

public class RoleService
{
    private readonly HttpClient _httpClient;

    public RoleService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("Api");
    }

    public async Task<Result<RangedData<RoleDto>>> GetAsync(int limit, int offset, string? search = null)
    {
        var query = $"api/admin/roles?limit={limit}&offset={offset}";

        if (!string.IsNullOrWhiteSpace(search))
            query += $"&search={Uri.EscapeDataString(search)}";

        var response = await _httpClient.GetAsync(query);

        var responseResult = await response.GetStatusResultAsync();

        if (responseResult.IsFailed)
            return Result.Fail(responseResult.Errors);

        var model = await response.Content.ReadFromJsonAsync<RangedData<RoleDto>>(DtoSerializer.Default.Options);

        if (model is null)
            return Result.Fail("Failed to deserialize model");

        return model;
    }

    public async Task<Result<RoleDto>> CreateAsync(CreateRoleDto requestDto)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/admin/roles");
        request.Content = JsonContent.Create(requestDto, DtoSerializer.Default.CreateRoleDto);

        using var response = await _httpClient.SendAsync(request);

        var responseResult = await response.GetStatusResultAsync();

        if (responseResult.IsFailed)
            return Result.Fail(responseResult.Errors);

        var model = await response.Content.ReadFromJsonAsync<RoleDto>(DtoSerializer.Default.Options);

        if (model is null)
            return Result.Fail("Failed to deserialize model");

        return model;
    }

    public async Task<Result<RoleDto>> UpdateAsync(int id, UpdateRoleDto requestDto)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"api/admin/roles/{id}");
        request.Content = JsonContent.Create(requestDto, DtoSerializer.Default.UpdateRoleDto);

        using var response = await _httpClient.SendAsync(request);

        var responseResult = await response.GetStatusResultAsync();

        if (responseResult.IsFailed)
            return Result.Fail(responseResult.Errors);

        var model = await response.Content.ReadFromJsonAsync<RoleDto>(DtoSerializer.Default.Options);

        if (model is null)
            return Result.Fail("Failed to deserialize model");

        return model;
    }

    public async Task<Result> DeleteAsync(int id)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/admin/roles/{id}");

        using var response = await _httpClient.SendAsync(request);

        return await response.GetStatusResultAsync();
    }

    public async Task<Result<RangedData<MembershipDto>>> GetMembersAsync(int roleId, int offset, int limit)
    {
        using var response =
            await _httpClient.GetAsync($"api/admin/roles/{roleId}/members?offset={offset}&limit={limit}");

        var responseResult = await response.GetStatusResultAsync();

        if (responseResult.IsFailed)
            return Result.Fail(responseResult.Errors);

        var model = await response.Content.ReadFromJsonAsync<RangedData<MembershipDto>>(DtoSerializer.Default.Options);

        if (model is null)
            return Result.Fail("Failed to deserialize model");

        return Result.Ok(model);
    }

    public async Task<Result> AddMemberAsync(int roleId, int userId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/admin/roles/{roleId}/members");
        
        request.Content = JsonContent.Create(
            new AddMembershipDto()
            {
                UserId = userId
            },
            DtoSerializer.Default.AddMembershipDto
        );

        using var response = await _httpClient.SendAsync(request);

        return await response.GetStatusResultAsync();
    }

    public async Task<Result> RemoveMemberAsync(int roleId, int userId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/admin/roles/{roleId}/members/{userId}");

        using var response = await _httpClient.SendAsync(request);

        return await response.GetStatusResultAsync();
    }
}