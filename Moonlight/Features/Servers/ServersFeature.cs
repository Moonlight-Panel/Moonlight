using MoonCore.Helpers;
using MoonCore.Services;
using Moonlight.Core.Configuration;
using Moonlight.Core.Interfaces;
using Moonlight.Core.Interfaces.Ui.Admin;
using Moonlight.Core.Interfaces.UI.User;
using Moonlight.Core.Models.Abstractions.Feature;
using Moonlight.Core.Services;
using Moonlight.Features.Servers.Actions;
using Moonlight.Features.Servers.Configuration;
using Moonlight.Features.Servers.Http.Middleware;
using Moonlight.Features.Servers.Implementations.AdminDashboard.Columns;
using Moonlight.Features.Servers.Implementations.AdminDashboard.Components;
using Moonlight.Features.Servers.Implementations.Diagnose;
using Moonlight.Features.Servers.Models.Enums;
using Moonlight.Features.Servers.Services;
using Moonlight.Features.Servers.UI.Components.Cards;
using UserDashboardServerCount = Moonlight.Features.Servers.Implementations.UserDashboard.Components.UserDashboardServerCount;

namespace Moonlight.Features.Servers;

public class ServersFeature : MoonlightFeature
{
    public ServersFeature()
    {
        Name = "Servers";
        Author = "MasuOwO and contributors";
        IssueTracker = "https://github.com/Moonlight-Panel/Moonlight/issues";
    }

    public override Task OnPreInitialized(PreInitContext context)
    {
        context.EnableDependencyInjection<ServersFeature>();
        
        //
        var config = new ConfigService<CoreConfiguration>(PathBuilder.File("storage", "configs", "core.json"));
        context.Builder.Services.AddSingleton(new JwtService<ServersJwtType>(config.Get().Security.Token, context.LoggerFactory.CreateLogger<JwtService<ServersJwtType>>()));
        
        //
        var configService = new ConfigService<ServersConfiguration>(PathBuilder.File("storage", "configs", "servers.json"));
        context.Builder.Services.AddSingleton(configService);
        
        // Assets
        context.AddAsset("Servers", "css/XtermBlazor.css");
        
        context.AddAsset("Servers", "css/apexcharts.css");
        
        context.AddAsset("Servers", "js/XtermBlazor.min.js");
        context.AddAsset("Servers", "js/xterm-addon-fit.min.js");
        context.AddAsset("Servers", "js/terminal.js");
        
        context.AddAsset("Servers", "js/apexcharts.esm.js");
        context.AddAsset("Servers", "js/blazor-apexcharts.js");
        
        return Task.CompletedTask;
    }

    public override async Task OnInitialized(InitContext context)
    {
        var app = context.Application;

        app.UseMiddleware<NodeMiddleware>();
        
        // Configure node startup
        var startupJobService = app.Services.GetRequiredService<StartupJobService>();

        await startupJobService.AddJob("Boot all server nodes", TimeSpan.FromSeconds(3), async provider =>
        {
            var nodeService = provider.GetRequiredService<NodeService>();
            await nodeService.BootAll();
        });
        
        // Configure schedule actions
        var serverScheduleService = app.Services.GetRequiredService<ServerScheduleService>();

        await serverScheduleService.RegisterAction<EnterConsoleInputAction>("enterConsoleInput");
        await serverScheduleService.RegisterAction<StartBackupAction>("startBackup");
        
        // Configure permissions
        var permissionService = app.Services.GetRequiredService<PermissionService>();

        await permissionService.Register(5000, new()
        {
            Name = "Manage servers",
            Description = "Allows access to every server, allows to create, delete and update servers"
        });
        
        await permissionService.Register(5001, new()
        {
            Name = "Manage server nodes",
            Description = "Allows access to the node settings and see information about the current status"
        });
        
        await permissionService.Register(5002, new()
        {
            Name = "Manage server images",
            Description = "Allows access to all images and allows to edit, update and delete them"
        });
        
        // Register diagnose actions via plugin hooks
        var pluginService = app.Services.GetRequiredService<PluginService>();

        await pluginService.RegisterImplementation<IDiagnoseAction>(new NodesDiagnoseAction());
        
        await pluginService.RegisterImplementation<IAdminDashboardColumn>(new ServerCount());
        
        await pluginService.RegisterImplementation<IAdminDashboardComponent>(new NodeOverview());
        
        await pluginService.RegisterImplementation<IUserDashboardComponent>(new UserDashboardServerCount());
    }

    public override Task OnUiInitialized(UiInitContext context)
    {
        context.EnablePages<ServersFeature>();
        
        context.AddSidebarItem("Servers", "bx-server", "/servers", isAdmin: false, needsExactMatch: false);
        context.AddSidebarItem("Servers", "bx-server", "/admin/servers", isAdmin: true, needsExactMatch: false);
        
        return Task.CompletedTask;
    }
}