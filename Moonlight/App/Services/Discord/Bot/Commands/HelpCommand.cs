using Discord;
using Discord.WebSocket;
using Moonlight.App.Services.Discord.Bot.Module;

namespace Moonlight.App.Services.Discord.Bot.Commands;

public class HelpCommand : BaseModule
{
    public HelpCommand(DiscordSocketClient client, ConfigService configService, IServiceScope scope) : base(client, configService, scope)
    { }

    public async Task Handler(SocketSlashCommand command)
    {
        var ebm = Scope.ServiceProvider.GetRequiredService<EmbedBuilderModule>();
    }

    public override Task Init()
    {
        throw new NotImplementedException();
    }
}