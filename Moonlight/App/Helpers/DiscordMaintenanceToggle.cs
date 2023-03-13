using Discord.WebSocket;
using Moonlight.App.Services.DiscordBot;

namespace Moonlight.App.Helpers;

public class DiscordMaintenanceToggle
{
    private Task MaintenanceModeToggle(SocketSlashCommand command)
    {
        if (DiscordBotService.MaintenanceMode)
        {
            DiscordBotService.MaintenanceMode = false;
        }
        else
        {
            DiscordBotService.MaintenanceMode = true;
        }
        return Task.CompletedTask;
    }

}