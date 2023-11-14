using Discord.WebSocket;

namespace Moonlight.App.Services.Discord.Bot.Commands;

public class CommandControllerModule : BaseModule
{
    public CommandControllerModule(DiscordSocketClient client, ConfigService configService, IServiceScope scope) : base(client, configService, scope)
    {
        Client.SlashCommandExecuted += OnSlashCommandExecuted;
    }

    private async Task OnSlashCommandExecuted(SocketSlashCommand command)
    {
        if(!ConfigService.Get().Discord.Bot.Enable == false) return;
        if(command.User.IsBot) return;
        var dsc = Scope.ServiceProvider.GetRequiredService<DiscordBotService>();
        
        //Global Commands
        switch (command.CommandName)
        {
            case "help": await dsc.HelpCommand.Handler(command); break;
        }
        
        //Guild Commands that can only be executed on the main Guild (Support Server)
        if(command.GuildId != (ulong)DiscordConfig.GuildId) return;
        switch (command.CommandName)
        {
            case "setup": await dsc.SetupCommand.Handler(command); break;
        }
    }

    public override Task Init()
        { throw new NotImplementedException(); }
}