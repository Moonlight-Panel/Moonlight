using MoonCore.Helpers;
using MoonCore.Services;
using Moonlight.Core.Configuration;
using Moonlight.Core.Models.Abstractions.Feature;
using Moonlight.Core.Services;
using Moonlight.Features.FileManager.Models.Enums;

namespace Moonlight.Features.FileManager;

public class FileManagerFeature : MoonlightFeature
{
    public FileManagerFeature()
    {
        Name = "File Manager Components";
        Author = "MasuOwO";
        IssueTracker = "https://github.com/Moonlight-Panel/Moonlight/issues";
    }

    public override Task OnPreInitialized(PreInitContext context)
    {
        context.EnableDependencyInjection<FileManagerFeature>();
        
        //
        var config = new ConfigService<CoreConfiguration>(PathBuilder.File("storage", "configs", "core.json"));
        context.Builder.Services.AddSingleton(new JwtService<FileManagerJwtType>(config.Get().Security.Token));
        
        context.AddAsset("FileManager", "js/dropzone.js");
        context.AddAsset("FileManager", "editor/ace.css");
        context.AddAsset("FileManager", "editor/ace.js");
        
        return Task.CompletedTask;
    }

    public override async Task OnSessionInitialized(SessionInitContext context)
    {
        var hotKeyService = context.ServiceProvider.GetRequiredService<HotKeyService>();

        await hotKeyService.RegisterHotkey("KeyS", "ctrl", "save");
        await hotKeyService.RegisterHotkey("KeyX", "ctrl", "close");
    }
}