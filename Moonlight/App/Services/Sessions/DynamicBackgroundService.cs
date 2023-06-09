using Logging.Net;
using Moonlight.App.Services.Files;

namespace Moonlight.App.Services.Sessions;

public class DynamicBackgroundService
{
    public EventHandler OnBackgroundImageChanged { get; set; }
    public string BackgroundImageUrl { get; private set; }
    private string DefaultBackgroundImageUrl;

    public DynamicBackgroundService(ResourceService resourceService)
    {
        DefaultBackgroundImageUrl = resourceService.BackgroundImage("main.jpg");
        BackgroundImageUrl = DefaultBackgroundImageUrl;
    }

    public Task Change(string url)
    {
        BackgroundImageUrl = url;
        OnBackgroundImageChanged?.Invoke(this, null!);

        return Task.CompletedTask;
    }

    public Task Reset()
    {
        BackgroundImageUrl = DefaultBackgroundImageUrl;
        OnBackgroundImageChanged?.Invoke(this, null!);

        return Task.CompletedTask;
    }
}