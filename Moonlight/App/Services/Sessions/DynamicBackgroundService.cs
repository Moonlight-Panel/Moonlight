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
        if(BackgroundImageUrl == url) // Prevent unnecessary updates
            return Task.CompletedTask;
        
        BackgroundImageUrl = url;
        OnBackgroundImageChanged?.Invoke(this, null!);

        return Task.CompletedTask;
    }

    public Task Reset()
    {
        if(BackgroundImageUrl == DefaultBackgroundImageUrl) // Prevent unnecessary updates
            return Task.CompletedTask;
        
        BackgroundImageUrl = DefaultBackgroundImageUrl;
        OnBackgroundImageChanged?.Invoke(this, null!);

        return Task.CompletedTask;
    }
}