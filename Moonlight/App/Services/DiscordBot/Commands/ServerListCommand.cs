using Discord;
using Discord.WebSocket;
using Logging.Net;
using Microsoft.EntityFrameworkCore;
using Moonlight.App.Repositories;
using Moonlight.App.Repositories.Servers;


namespace Moonlight.App.Services.DiscordBot.Commands;

public class ServerListCommand : BaseModule
{
    public ServerListCommand(DiscordSocketClient client, ConfigService configService, IServiceScope scope) : base(client, configService, scope)
    {
        Client.SlashCommandExecuted += ServerMenuCommand;
    }
    private async Task ServerMenuCommand(SocketSlashCommand command)
    {
        EmbedBuilder embed;
        ComponentBuilder components;
        var dcs = Scope.ServiceProvider.GetRequiredService<DiscordBotService>();
        
        if (command.User.IsBot) return;
        if (command.CommandName != "servers") return;
        var usersRepo = Scope.ServiceProvider.GetRequiredService<UserRepository>();
        var user = usersRepo.Get().FirstOrDefault(x => x.DiscordId == command.User.Id);
        //var user = usersRepo.Get().FirstOrDefault(x => x.Id == 1);

        if (user == null)
        {
            embed = dcs.EmbedBuilderModule.StandardEmbed("Sorry ;( \n Please first create and/or link a Account to Discord! \n Press the Button to register/log in.", Color.Red, command.User);
            components = new ComponentBuilder();
            components.WithButton("Click Here", style: ButtonStyle.Link, url: ConfigService.GetSection("Moonlight").GetValue<String>("AppUrl"));
        
            await command.RespondAsync(embed: embed.Build(), components: components.Build(), ephemeral: true);
            return;
        }
        var serversRepo = Scope.ServiceProvider.GetRequiredService<ServerRepository>();
        var servers = serversRepo.Get().Include(x => x.Owner).Where(x => x.Owner.Id == user.Id).ToList();
        
        var selectOptions = new List<SelectMenuOptionBuilder>();
        
        foreach (var server in servers.Take(25))
        {
            selectOptions.Add(new SelectMenuOptionBuilder()
                .WithLabel($"{server.Id} - {server.Name}")
                .WithEmote(Emote.Parse("<:server3:968614410228736070>"))
                .WithValue(server.Id.ToString()));
        }
        
        components = new ComponentBuilder();
        
        components.WithSelectMenu(
            "ServerSelectorList",
            selectOptions,
            "Select the server you want to edit.");
        
        components.WithButton("Panel",
            emote: Emote.Parse("<a:Earth:1092814004113657927>"),
            style: ButtonStyle.Link,
            url: $"{ConfigService.GetSection("Moonlight").GetValue<string>("AppUrl")}");
        
        if (servers.Count > 25)
        {
            components.WithButton("Previous-page",
                emote: Emote.Parse("<:arrowLeft:1101182242908278817>"),
                style: ButtonStyle.Secondary,
                customId:"SmPreviousPage.0",
                disabled: true);
            
            components.WithButton("Next-page",
                emote: Emote.Parse("<:arrowRight:1101182245408096336>"),
                style: ButtonStyle.Secondary, 
                customId:"SmNextPage.0",
                disabled: false);
        }
        
        embed = dcs.EmbedBuilderModule.StandardEmbed("Server Manager", Color.Blue, command.User);
        
        
        await command.RespondAsync(embed: embed.Build(), components: components.Build(), ephemeral: true);
    }
    
    public override async Task RegisterCommands()
    {
        var command = new SlashCommandBuilder()
            .WithName("servers")
            .WithDescription("Creates a list from all your servers");

        await Client.CreateGlobalApplicationCommandAsync(command.Build());
    }
}