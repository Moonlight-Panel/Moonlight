using Moonlight.Features.Servers.Entities;
using Moonlight.Features.Servers.Models.Abstractions;
using Moonlight.Features.Servers.Models.Forms.Schedules;
using Moonlight.Features.Servers.Services;

namespace Moonlight.Features.Servers.Actions;

public class EnterConsoleInputAction : ScheduleAction
{
    public EnterConsoleInputAction()
    {
        DisplayName = "Enter console input";
        Icon = "bxs-terminal";
        FormType = typeof(EnterConsoleInputForm);
    }
    
    public override async Task Execute(Server server, object config, IServiceProvider serviceProvider)
    {
        var configData = config as EnterConsoleInputForm;
        
        var serverService = serviceProvider.GetRequiredService<ServerService>();
        await serverService.Console.SendCommand(server, configData!.Input);
    }
}