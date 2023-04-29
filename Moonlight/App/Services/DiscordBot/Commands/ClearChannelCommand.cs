using Discord;
using Discord.WebSocket;
using Logging.Net;

namespace Moonlight.App.Services.DiscordBot.Commands;

public class ClearChannelCommand : BaseModule
{
    public ClearChannelCommand(DiscordSocketClient client, ConfigService configService, IServiceScope scope) : base(client, configService, scope)
    {
        Client.SlashCommandExecuted += ClearChannel;
        Client.ButtonExecuted += ButtonHandler;
    }
    
    private async Task ClearChannel(SocketSlashCommand command)
    {
        EmbedBuilder embed;   
        if(command.User.IsBot) return;
        if(command.CommandName != "clear") return;
        if(command.IsDMInteraction)
        {
            embed = new EmbedBuilder()
                .WithAuthor($"Clear DM's > {command.User.Username}", Client.CurrentUser.GetAvatarUrl())
                .WithDescription($"Please press the button below. **This can't be undone!**")
                .WithColor(Color.Purple);

            var button = new ComponentBuilder()
                .WithButton("Clear", emote: new Emoji("🚮"), style: ButtonStyle.Danger, customId: $"clearDM");
            
            await command.RespondAsync(embed: embed.Build(), components: button.Build());
        }
        else
        {
            embed = new EmbedBuilder()
                .WithAuthor($"Error > {command.User.Username}", Client.CurrentUser.GetAvatarUrl())
                .WithDescription($"Please use this Command here in your DM's")
                .WithColor(Color.Red);
            
            await command.RespondAsync(embed: embed.Build(),ephemeral: true);
        }
    }
    
    private async Task ButtonHandler(SocketMessageComponent component)
    {
        if (component.Data.CustomId == "clearDM")
        {
            var button = new ComponentBuilder()
                .WithButton("Clear", emote: new Emoji("🚮"), style: ButtonStyle.Danger, customId: $"clearDM", disabled: true);

            await component.RespondAsync("Please wait...", ephemeral: true);
            
            ulong userId = component.User.Id;

            int messagesDeleted = 0;
            var dmChannel = await component.User.CreateDMChannelAsync();
            IEnumerable<IMessage> messages = await dmChannel.GetMessagesAsync().FlattenAsync();

            foreach (var message in messages)
            {
                if (message.Author.IsBot)
                {
                    await message.DeleteAsync();
                    await Task.Delay(500);
                    messagesDeleted++;
                }
            }

            var embed = new EmbedBuilder()
                .WithAuthor("Messages Delete!", Client.CurrentUser.GetAvatarUrl())
                .WithDescription($"I deleted {messagesDeleted} messages.")
                .WithColor(Color.Green);

            IUserMessage response = await dmChannel.SendMessageAsync(embed: embed.Build());
            await Task.Delay(TimeSpan.FromSeconds(5));
            await response.DeleteAsync();
        }
    }
    public async override Task RegisterCommands()
    {
        var command = new SlashCommandBuilder()
            .WithName("clear")
            .WithDescription("Clear your DM Channel");
        
        await Client.CreateGlobalApplicationCommandAsync(command.Build());
    }
}