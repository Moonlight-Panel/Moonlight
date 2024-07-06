using MoonCore.Helpers;
using MoonCore.Services;
using Moonlight.Core.Configuration;
using Moonlight.Core.Models.Abstractions.Feature;
using Moonlight.Core.Services;
using Moonlight.Features.FileManager.Implementations;
using Moonlight.Features.FileManager.Interfaces;
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
        
        context.Builder.Services.AddSingleton(
            new JwtService<FileManagerJwtType>(
                config.Get().Security.Token,
                context.LoggerFactory.CreateLogger<JwtService<FileManagerJwtType>>()
                )
        );
        
        context.AddAsset("FileManager", "js/filemanager.js");
        context.AddAsset("FileManager", "editor/ace.css");
        context.AddAsset("FileManager", "editor/ace.js");
        
        // Add blazor context menu
        context.Builder.Services.AddBlazorContextMenu(builder =>
        {
            /*
            builder.ConfigureTemplate(template =>
            {
                
            });*/
        });
        
        context.AddAsset("FileManager", "js/blazorContextMenu.js");
        context.AddAsset("FileManager", "css/blazorContextMenu.css");
        
        return Task.CompletedTask;
    }

    public override async Task OnInitialized(InitContext context)
    {
        // Register default file manager actions in plugin service
        var pluginService = context.Application.Services.GetRequiredService<PluginService>();

        await pluginService.RegisterImplementation<IFileManagerContextAction>(new RenameContextAction());
        await pluginService.RegisterImplementation<IFileManagerContextAction>(new MoveContextAction());
        await pluginService.RegisterImplementation<IFileManagerContextAction>(new DownloadContextAction());
        await pluginService.RegisterImplementation<IFileManagerContextAction>(new ArchiveContextAction());
        await pluginService.RegisterImplementation<IFileManagerContextAction>(new ExtractContextAction());
        await pluginService.RegisterImplementation<IFileManagerContextAction>(new DeleteContextAction());

        await pluginService.RegisterImplementation<IFileManagerSelectionAction>(new MoveSelectionAction());
        await pluginService.RegisterImplementation<IFileManagerSelectionAction>(new ArchiveSelectionAction());
        await pluginService.RegisterImplementation<IFileManagerSelectionAction>(new DeleteSelectionAction());

        await pluginService.RegisterImplementation<IFileManagerCreateAction>(new CreateFileAction());
        await pluginService.RegisterImplementation<IFileManagerCreateAction>(new CreateFolderAction());
    }

    public override async Task OnSessionInitialized(SessionInitContext context)
    {
        // Register hotkeys
        var hotKeyService = context.ServiceProvider.GetRequiredService<HotKeyService>();

        await hotKeyService.RegisterHotkey("KeyS", "ctrl", "save");
        await hotKeyService.RegisterHotkey("KeyX", "ctrl", "close");
    }
}