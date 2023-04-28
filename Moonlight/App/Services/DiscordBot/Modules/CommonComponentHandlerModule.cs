using Discord;
using Discord.WebSocket;

namespace Moonlight.App.Services.DiscordBot.Modules;

public class CommonComponentHandlerModule : BaseModule
{
    public CommonComponentHandlerModule(DiscordSocketClient client, ConfigService configService, IServiceScope scope) : base(client, configService, scope)
    {
        Client.ButtonExecuted += CommonButtonHandler;
    }
    public override Task RegisterCommands()
        { return Task.CompletedTask; }
    
    private async Task CommonButtonHandler(SocketMessageComponent component)
    {
        EmbedBuilder embed;
        switch (component.Data.CustomId)
        {
            case "clear":
                await component.Message.DeleteAsync();
                var dcs = Scope.ServiceProvider.GetRequiredService<DiscordBotService>();
                embed = dcs.EmbedBuilderModule.StandardEmbed("Cleared!", Color.Green, component.User);
                await component.RespondAsync(embed: embed.Build(), ephemeral: true);
                
                break;
        }
    }
}