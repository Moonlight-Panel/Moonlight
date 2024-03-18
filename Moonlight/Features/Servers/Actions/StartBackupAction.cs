using Moonlight.Features.Servers.Entities;
using Moonlight.Features.Servers.Models.Abstractions;
using Moonlight.Features.Servers.Services;

namespace Moonlight.Features.Servers.Actions;

public class StartBackupAction : ScheduleAction
{
    public StartBackupAction()
    {
        DisplayName = "Start creating a backup";
        Icon = "bxs-box";
        FormType = typeof(object);
    }
    
    public override async Task Execute(Server server, object config, IServiceProvider serviceProvider)
    {
        var serverBackupService = serviceProvider.GetRequiredService<ServerBackupService>();
        await serverBackupService.Create(server);
    }
}