using MoonCore.Attributes;
using MoonCore.Helpers;
using MoonCore.Models;
using Moonlight.Shared.Http.Requests.Admin.Sys.Theme;
using Moonlight.Shared.Http.Responses.Admin;
using Moonlight.Shared.Misc;

namespace Moonlight.Client.Services;

[Scoped]
public class ThemeService
{
    private readonly HttpApiClient ApiClient;

    public ThemeService(HttpApiClient apiClient)
    {
        ApiClient = apiClient;
    }

    public async Task<PagedData<ThemeResponse>> Get(int page, int pageSize)
    {
        return await ApiClient.GetJson<PagedData<ThemeResponse>>(
            $"api/admin/system/customisation/themes?page={page}&pageSize={pageSize}"
        );
    }

    public async Task<ThemeResponse> Get(int id)
    {
        return await ApiClient.GetJson<ThemeResponse>(
            $"api/admin/system/customisation/themes/{id}"
        );
    }

    public async Task<ThemeResponse> Create(CreateThemeRequest request)
    {
        return await ApiClient.PostJson<ThemeResponse>(
            "api/admin/system/customisation/themes",
            request
        );
    }

    public async Task<ThemeResponse> Update(int id, UpdateThemeRequest request)
    {
        return await ApiClient.PatchJson<ThemeResponse>(
            $"api/admin/system/customisation/themes/{id}",
            request
        );
    }

    public async Task Delete(int id)
    {
        await ApiClient.Delete(
            $"api/admin/system/customisation/themes/{id}"
        );
    }
}