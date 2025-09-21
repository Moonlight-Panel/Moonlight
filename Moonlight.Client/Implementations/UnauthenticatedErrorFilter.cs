using Microsoft.AspNetCore.Components;
using MoonCore.Blazor.FlyonUi.Exceptions;
using MoonCore.Blazor.FlyonUi.Toasts;
using MoonCore.Exceptions;

namespace Moonlight.Client.Implementations;

public class UnauthenticatedErrorFilter : IGlobalErrorFilter
{
    private readonly NavigationManager Navigation;
    private readonly ToastService ToastService;

    public UnauthenticatedErrorFilter(
        NavigationManager navigation,
        ToastService toastService
    )
    {
        Navigation = navigation;
        ToastService = toastService;
    }

    public async Task<bool> HandleExceptionAsync(Exception ex)
    {
        if (ex is not HttpApiException { Status: 401 })
            return false;

        await ToastService.InfoAsync("Session expired", "Your session has expired. Reloading..");
        
        Navigation.NavigateTo("/api/auth/logout", true);
        return true;
    }
}