using MoonCore.Attributes;
using MoonCore.Helpers;
using Moonlight.Shared.Http.Requests.Admin.Sys.Theme;
using Moonlight.Shared.Http.Responses.Admin;

namespace Moonlight.Client.Services;

[Scoped]
public class ThemeService
{
    private readonly HttpApiClient ApiClient;

    public ThemeService(HttpApiClient apiClient)
    {
        ApiClient = apiClient;
    }

    public async Task<ThemeResponse> GetAsync(int id)
    {
        return await ApiClient.GetJson<ThemeResponse>(
            $"api/admin/system/customisation/themes/{id}"
        );
    }

    public async Task<ThemeResponse> CreateAsync(CreateThemeRequest request)
    {
        return await ApiClient.PostJson<ThemeResponse>(
            "api/admin/system/customisation/themes",
            request
        );
    }

    public async Task<ThemeResponse> UpdateAsync(int id, UpdateThemeRequest request)
    {
        return await ApiClient.PatchJson<ThemeResponse>(
            $"api/admin/system/customisation/themes/{id}",
            request
        );
    }

    public async Task DeleteAsync(int id)
    {
        await ApiClient.Delete(
            $"api/admin/system/customisation/themes/{id}"
        );
    }
}