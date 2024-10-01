using MoonCore.Blazor.Tailwind.Services;
using MoonCore.Exceptions;
using MoonCore.Helpers;
using Moonlight.Shared.Http.Responses.Auth;

namespace Moonlight.Client.Services;

public class IdentityService
{
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string[] Permissions { get; private set; }
    public bool IsLoggedIn { get; private set; }

    public event Func<Task> OnStateChanged;

    private readonly CookieService CookieService;
    private readonly HttpApiClient ApiClient;
    
    public IdentityService(CookieService cookieService, HttpApiClient apiClient)
    {
        CookieService = cookieService;
        ApiClient = apiClient;
    }

    public async Task Check()
    {
        try
        {
            var response = await ApiClient.GetJson<CheckResponse>("api/auth/check");

            Username = response.Username;
            Email = response.Email;
            Permissions = response.Permissions;

            IsLoggedIn = true;
        }
        catch (HttpApiException)
        {
            IsLoggedIn = false;
        }

        await OnStateChanged();
    }

    public async Task Login(string token)
    {
        await CookieService.SetValue("token", token, 30);
        await Check();
    }

    public async Task Logout()
    {
        await CookieService.SetValue("token", "", 30);
        await Check();
    }
}