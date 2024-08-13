using MoonCore.Attributes;
using Moonlight.Client.App.Models.Modals;
using Moonlight.Client.App.UI.Components.Modals;

namespace Moonlight.Client.App.Services;

[Scoped]
public class ModalService
{
    private ModalLaunchPoint? LaunchPoint = null;

    public async Task Hide(ModalLaunchItem item)
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

    public Task SetLaunchPoint(ModalLaunchPoint launchPoint)
    {
        LaunchPoint = launchPoint;
        return Task.CompletedTask;
    }

    public async Task Launch<T>(string? id = null, Action<Dictionary<string, object>>? buildAttributes = null) where T : BaseModal
    {
        if (LaunchPoint == null)
        {
            throw new ArgumentNullException(nameof(LaunchPoint),
                "You need to have a launch point initialized before using this function");
        }

        await LaunchPoint.Launch<T>(id, buildAttributes);
    }
}