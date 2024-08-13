using MoonCore.Attributes;
using Moonlight.Client.App.Models.Toasts;
using Moonlight.Client.App.UI.Components.Toasts;

namespace Moonlight.Client.App.Services;

[Scoped]
public class ToastService
{
    private ToastLaunchPoint? LaunchPoint = null;

    public async Task Hide(ToastLaunchItem item)
    {
        if (LaunchPoint == null)
        {
            throw new ArgumentNullException(nameof(LaunchPoint),
                "You need to have a launch point initialized before using this function");
        }

        await LaunchPoint.Hide(item);
    }
    
    public async Task Hide(string id)
    {
        if (LaunchPoint == null)
        {
            throw new ArgumentNullException(nameof(LaunchPoint),
                "You need to have a launch point initialized before using this function");
        }

        await LaunchPoint.Hide(id);
    }

    public Task SetLaunchPoint(ToastLaunchPoint launchPoint)
    {
        LaunchPoint = launchPoint;
        return Task.CompletedTask;
    }

    public async Task Launch<T>(string? id = null, bool enableAutoDisappear = true, Action<Dictionary<string, object>>? buildAttributes = null) where T : BaseToast
    {
        if (LaunchPoint == null)
        {
            throw new ArgumentNullException(nameof(LaunchPoint),
                "You need to have a launch point initialized before using this function");
        }

        await LaunchPoint.Launch<T>(id, enableAutoDisappear, buildAttributes);
    }

    #region Launchers

    public Task Information(string text) => LaunchInternal<InformationToast>(text);
    public Task Success(string text) => LaunchInternal<SuccessToast>(text);
    public Task Warning(string text) => LaunchInternal<WarningToast>(text);
    public Task Error(string text) => LaunchInternal<ErrorToast>(text);

    private async Task LaunchInternal<T>(string text) where T : BaseToast
    {
        await Launch<T>(buildAttributes: parameters =>
        {
            parameters.Add("Text", text);
        });
    }

    #endregion
}