using System.Security.Claims;
using System.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MoonCore.Blazor.FlyonUi.Auth;
using MoonCore.Blazor.Services;
using MoonCore.Exceptions;
using MoonCore.Helpers;
using Moonlight.Shared.Http.Requests.Auth;
using Moonlight.Shared.Http.Responses.Auth;

namespace Moonlight.Client.Services;

public class RemoteAuthStateManager : AuthenticationStateManager
{
    private readonly NavigationManager NavigationManager;
    private readonly HttpApiClient HttpApiClient;
    private readonly LocalStorageService LocalStorageService;
    private readonly ILogger<RemoteAuthStateManager> Logger;

    public RemoteAuthStateManager(
        HttpApiClient httpApiClient,
        LocalStorageService localStorageService,
        NavigationManager navigationManager,
        ILogger<RemoteAuthStateManager> logger
    )
    {
        HttpApiClient = httpApiClient;
        LocalStorageService = localStorageService;
        NavigationManager = navigationManager;
        Logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        => await LoadAuthState();

    public override async Task HandleLogin()
    {
        var uri = new Uri(NavigationManager.Uri);
        var codeParam = HttpUtility.ParseQueryString(uri.Query).Get("code");

        if (string.IsNullOrEmpty(codeParam)) // If this is true, we need to log in the user
        {
            await StartLogin();
        }
        else
        {
            try
            {
                var loginCompleteData = await HttpApiClient.PostJson<LoginCompleteResponse>(
                    "api/auth/complete",
                    new LoginCompleteRequest()
                    {
                        Code = codeParam
                    }
                );

                await LocalStorageService.SetString("AccessToken", loginCompleteData.AccessToken);
                
                NavigationManager.NavigateTo("/");
                NotifyAuthenticationStateChanged(LoadAuthState());
            }
            catch (HttpApiException e)
            {
                Logger.LogError("Unable to complete login: {e}", e);

                await StartLogin();
            }
        }
    }

    public override async Task Logout()
    {
        if (await LocalStorageService.ContainsKey("AccessToken"))
            await LocalStorageService.SetString("AccessToken", "");
        
        NotifyAuthenticationStateChanged(LoadAuthState());
    }

    #region Utilities

    private async Task StartLogin()
    {
        var loginStartData = await HttpApiClient.GetJson<LoginStartResponse>("api/auth/start");

        var url = $"{loginStartData.Endpoint}" +
                  $"?client_id={loginStartData.ClientId}" +
                  $"&redirect_uri={loginStartData.RedirectUri}" +
                  $"&response_type=code";

        NavigationManager.NavigateTo(url, true);
    }

    private async Task<AuthenticationState> LoadAuthState()
    {
        AuthenticationState newState;

        try
        {
            var checkData = await HttpApiClient.GetJson<CheckResponse>("api/auth/check");

            newState = new(new ClaimsPrincipal(
                new ClaimsIdentity(
                    [
                        new Claim("username", checkData.Username),
                        new Claim("email", checkData.Email),
                        new Claim("permissions", checkData.Permissions)
                    ],
                    "RemoteAuthStateManager"
                )
            ));
        }
        catch (HttpApiException)
        {
            newState = new(new ClaimsPrincipal(
                new ClaimsIdentity()
            ));
        }

        return newState;
    }

    #endregion
}