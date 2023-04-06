using System.Diagnostics;
using Discord;
using Discord.WebSocket;
using Logging.Net;

namespace Moonlight.App.Services.DiscordBot.Commands;

public class RemoveCommandsModuels : BaseModule
{
    public RemoveCommandsModuels(DiscordSocketClient client, ConfigService configService, IServiceScope scope) : base(client, configService, scope) {}
    public override Task RegisterCommands()
        { return Task.CompletedTask; }
    
    private async void VoidCommands()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        
        var commands = await Client.GetGlobalApplicationCommandsAsync();
        if (commands == null) return;

        foreach (var slashCommand in commands)
        {
            if(slashCommand.Name == "commands") continue;
            
            await slashCommand.DeleteAsync();
            Logger.Debug($"Deleted {slashCommand.Name}, {slashCommand.Id}");
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
        }
        
        stopwatch.Stop();
        Logger.Info($"Deleted all commands. Done in {stopwatch.ElapsedMilliseconds}ms");
    }
}