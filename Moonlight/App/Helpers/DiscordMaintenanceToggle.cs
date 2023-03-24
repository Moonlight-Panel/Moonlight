using Discord.WebSocket;
using Moonlight.App.Services.DiscordBot;

namespace Moonlight.App.Helpers;

public class DiscordMaintenanceToggle
{
    private Task MaintenanceModeToggle(SocketSlashCommand command)
    {
        DiscordBotService.MaintenanceMode = !DiscordBotService.MaintenanceMode;
        return Task.CompletedTask;
    }
}
