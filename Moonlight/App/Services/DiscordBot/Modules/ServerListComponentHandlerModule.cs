using Discord;
using Discord.WebSocket;
using Logging.Net;
using Microsoft.EntityFrameworkCore;
using Moonlight.App.ApiClients.Wings;
using Moonlight.App.Repositories;
using Moonlight.App.Repositories.Servers;

namespace Moonlight.App.Services.DiscordBot.Modules;

public class ServerListComponentHandlerModule : BaseModule
{
    public ServerListComponentHandlerModule(DiscordSocketClient client, ConfigService configService, IServiceScope scope) : base(client, configService, scope)
    {
        Client.SelectMenuExecuted += MenuHandler;
        Client.ButtonExecuted += ManagerButtonHandler;
        Client.ButtonExecuted += PagesButtonHandler;
        Client.ModalSubmitted += ModalHandler;
    }
    
    public override Task RegisterCommands()
    {
        return Task.CompletedTask;
    }

    private async Task ManagerButtonHandler(SocketMessageComponent component)
    {
        var nodeService = Scope.ServiceProvider.GetRequiredService<NodeService>();
        var serverRepo = Scope.ServiceProvider.GetRequiredService<ServerRepository>();
        var dcs = Scope.ServiceProvider.GetRequiredService<DiscordBotService>();
        var costomId = component.Data.CustomId.Split(".");
        EmbedBuilder embed = dcs.EmbedBuilderModule.StandardEmbed("Something went terribly wrong! \n Mission failed please try again later.", Color.Red, component.User);

        if (costomId.Length < 3) return;
        
        if(costomId[0] is not "Sm") return;
        
        int id = int.Parse(costomId[2]);
        var server = serverRepo.Get()
            .Include(x => x.Owner)
            .Include(x => x.Node)
            .Include(x => x.MainAllocation)
            .FirstOrDefault(x => x.Id == id);
        
        if (server == null)
        {
            await ErrorEmbedSnippet(component);
            return;
        }

        if (server.Owner.DiscordId != component.User.Id)
        {
            embed = dcs.EmbedBuilderModule.StandardEmbed("Is this your Server? I don't think so. \n Yes i did think of that.", Color.Red, component.User);
            await component.RespondAsync(embed: embed.Build(), ephemeral: true);
            return;
        }

        var data = await nodeService.GetStatus(server.Node);

        if (data == null)
        {
            await component.RespondAsync(embed: embed.Build(), ephemeral: true);
            return;
        }
        var serverService = Scope.ServiceProvider.GetRequiredService<ServerService>();
        var serverDetails = await serverService.GetDetails(server);
        
        // serverDetails.State == "STATE"
        // here a secret for masu :) look at the number on the left <<<---
        // starting
        // running
        // stopping
        // offline
        // installing
        if (!ConfigService.GetSection("Moonlight").GetSection("DiscordBot").GetValue<bool>("PowerActions") && costomId[1] is "Start" or "Restart" or "Stop" or "Kill" or "Update")
        {
            embed = dcs.EmbedBuilderModule.StandardEmbed($"This feature is disabled for Security reasons! \n If you believe this is a error please contact the Administrators from this panel.", Color.Red, component.User);
            await component.RespondAsync(embed: embed.Build(), ephemeral: true);
            await component.DeleteOriginalResponseAsync();
            return;
        }
        
        if (!ConfigService.GetSection("Moonlight").GetSection("DiscordBot").GetValue<bool>("SendCommands") && costomId[1] is "SendCommand")
        {
            embed = dcs.EmbedBuilderModule.StandardEmbed($"This feature is disabled for Security reasons! \n If you believe this is a error please contact the Administrators from this panel.", Color.Red, component.User);
            await component.RespondAsync(embed: embed.Build(), ephemeral: true);
            await component.DeleteOriginalResponseAsync();
            return;
        }

        if (serverDetails.State == "installing")
        {
            embed = dcs.EmbedBuilderModule.ColorChangerServerManagerEmbed(component.Message.Embeds.FirstOrDefault(), Color.Blue, component.User);
            await component.ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
        
            embed = dcs.EmbedBuilderModule.StandardEmbed("Server is in Installing \n please try again later.", Color.Blue, component.User);
            await component.RespondAsync(embed: embed.Build(), ephemeral: true);
            return;
        }

        switch (costomId[1])
        {
            case "Start":
                if (serverDetails.State != "offline")
                {
                    embed = dcs.EmbedBuilderModule.StandardEmbed("Server is in a Invalid State \n please try again later.", Color.Red, component.User);
                    await component.RespondAsync(embed: embed.Build(), ephemeral: true);
                    return;
                }
                
                await serverService.SetPowerState(server, PowerSignal.Start);
                
                await component.DeferAsync();
                embed = dcs.EmbedBuilderModule.ColorChangerServerManagerEmbed(component.Message.Embeds.FirstOrDefault(), Color.Green, component.User);
                await component.ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
                
                break;

        
            case "Restart":
                if (serverDetails.State == "running")
                {
                    embed = dcs.EmbedBuilderModule.StandardEmbed("Server is in a Invalid State \n please try again later.", Color.Red, component.User);
                    await component.RespondAsync(embed: embed.Build(), ephemeral: true);
                    return;
                }
                
                await serverService.SetPowerState(server, PowerSignal.Restart);
                
                await component.DeferAsync();
                embed = dcs.EmbedBuilderModule.ColorChangerServerManagerEmbed(component.Message.Embeds.FirstOrDefault(), Color.Orange, component.User);
                await component.ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
                
                break;

        
            case "Stop":
                if (serverDetails.State is not ("starting" or "stopping"))
                {
                    embed = dcs.EmbedBuilderModule.StandardEmbed("Server is in a Invalid State \n please try again later.", Color.Red, component.User);
                    await component.RespondAsync(embed: embed.Build(), ephemeral: true);
                    return;
                }
                
                await serverService.SetPowerState(server, PowerSignal.Stop);
                
                await component.DeferAsync();
                embed = dcs.EmbedBuilderModule.ColorChangerServerManagerEmbed(component.Message.Embeds.FirstOrDefault(), Color.Red, component.User);
                await component.ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
                
                break;

        
            case "Kill":
                if (serverDetails.State != "stopping")
                {
                    embed = dcs.EmbedBuilderModule.StandardEmbed("Server is in a Invalid State \n please try again later.", Color.Red, component.User);
                    await component.RespondAsync(embed: embed.Build(), ephemeral: true);
                    return;
                }
                
                await serverService.SetPowerState(server, PowerSignal.Kill);
                
                await component.DeferAsync();
                embed = dcs.EmbedBuilderModule.ColorChangerServerManagerEmbed(component.Message.Embeds.FirstOrDefault(), Color.DarkRed, component.User);
                await component.ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
                
                break;
            
            case "SendCommand":
                if (serverDetails.State != "running")
                {
                    embed = dcs.EmbedBuilderModule.StandardEmbed("Server is not Online! \n The server must be Online", Color.Red, component.User);
                    await component.RespondAsync(embed: embed.Build(), ephemeral: true);
                    return;
                }

                ModalBuilder modal = new ModalBuilder()
                    .WithTitle("Send Command To Server")
                    .WithCustomId($"Sm.SendCommand.{costomId[2]}")
                    .AddTextInput("Command", "Command", TextInputStyle.Short, "Type here Your Command", 1, 169, true);

                await component.RespondWithModalAsync(modal.Build());
                embed = dcs.EmbedBuilderModule.ColorChangerServerManagerEmbed(component.Message.Embeds.FirstOrDefault(), Color.Green, component.User);
                await component.ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
                break;

            case "Update":
                Color serverStateColor;
                switch (serverDetails.State)
                {
                    case "starting":
                        serverStateColor = Color.Orange;
                        break;
                
                    case "running":
                        serverStateColor = Color.Green;
                        break;
                
                    case "stopping":
                        serverStateColor = Color.Orange;
                        break;
                
                    case "offline":
                        serverStateColor = Color.DarkerGrey;
                        break;

                    default:
                        serverStateColor = Color.DarkPurple;
                        break;
                }
                await component.DeferAsync();
                embed = dcs.EmbedBuilderModule.ColorChangerServerManagerEmbed(component.Message.Embeds.FirstOrDefault(), serverStateColor, component.User);
                await component.ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
                break;
        }
    }
    
    private async Task ModalHandler(SocketModal component)
    {
        var nodeService = Scope.ServiceProvider.GetRequiredService<NodeService>();
        var serverRepo = Scope.ServiceProvider.GetRequiredService<ServerRepository>();
        var dcs = Scope.ServiceProvider.GetRequiredService<DiscordBotService>();
        var costomId = component.Data.CustomId.Split(".");
        EmbedBuilder embed;
        
        if(costomId[0] != "Sm" && costomId[1] == "SendCommand") return;
        
        var details = component.Data.Components.FirstOrDefault(x => x.CustomId == "Command");

        int id = int.Parse(costomId[2]);
        var server = serverRepo.Get()
            .Include(x => x.Owner)
            .Include(x => x.Node)
            .Include(x => x.MainAllocation)
            .FirstOrDefault(x => x.Id == id);
        
        if (server == null || details == null)
        {
            embed = dcs.EmbedBuilderModule.StandardEmbed("Sorry :( \n Something went wrong. \n Please try again later.", Color.Red, component.User);
            await component.RespondAsync(embed: embed.Build(), ephemeral: true);
            return;
        }

        if (server.Owner.DiscordId != component.User.Id)
        {
            embed = dcs.EmbedBuilderModule.StandardEmbed("Is this your Server? I don't think so. \n Yes i did think of that.", Color.Red, component.User);
            await component.RespondAsync(embed: embed.Build(), ephemeral: true);
            return;
        }

        var data = await nodeService.GetStatus(server.Node);
        if (data == null)
        {
            embed = dcs.EmbedBuilderModule.StandardEmbed("The node might be down. \n Please try again later.", Color.Red, component.User);
            await component.RespondAsync(embed: embed.Build(), ephemeral: true);
            return;
        }
        
        var serverService = Scope.ServiceProvider.GetRequiredService<ServerService>();
        Logger.Info(server.Id + " - " + server.Name);
        Logger.Info(details.Value);
        //Execute in console.
        
        embed = dcs.EmbedBuilderModule.StandardEmbed($"This feature is disabled for Security reasons! \n If you believe this is a error please contact the Administrators from this panel.", Color.Red, component.User);
        await component.RespondAsync(embed: embed.Build(), ephemeral: true);
        await component.DeleteOriginalResponseAsync();
        return;




    }
    
    private async Task PagesButtonHandler(SocketMessageComponent component)
    {
        ComponentBuilder components;
        EmbedBuilder embed;
        var dcs = Scope.ServiceProvider.GetRequiredService<DiscordBotService>();
        var usersRepo = Scope.ServiceProvider.GetRequiredService<UserRepository>();
        var serverRepo = Scope.ServiceProvider.GetRequiredService<ServerRepository>();
        var user = usersRepo.Get().FirstOrDefault(x => x.DiscordId == component.User.Id);
        var costomId = component.Data.CustomId.Split(".");
        
        if (costomId.Length < 2) return;
        int nextPage;
        switch (costomId[0])
        {
            case "SmPreviousPage":
                nextPage = int.Parse(costomId[1]) -1;
                break;
            
            case "SmNextPage":
                nextPage = int.Parse(costomId[1]) +1;
                break;
            
            default:
                return;
        }
        
        if (user == null)
        {
            embed = dcs.EmbedBuilderModule.StandardEmbed("Sorry ;( \n Please first create and/or link a Account to Discord! \n Press the Button to register/log in.", Color.Red, component.User);
            components = new ComponentBuilder();
            components.WithButton("Click Here", style: ButtonStyle.Link, url: ConfigService.GetSection("Moonlight").GetValue<String>("AppUrl"));
        
            await component.RespondAsync(embed: embed.Build(), components: components.Build(), ephemeral: true);
            return;
        }
        
        var servers = serverRepo.Get().Include(x => x.Owner).Where(x => x.Owner.Id == user.Id).ToList();
        var selectOptions = new List<SelectMenuOptionBuilder>();
        
        foreach (var server in servers.Skip(nextPage * 25).Take(25).ToList())
        {
            selectOptions.Add(new SelectMenuOptionBuilder()
                .WithLabel($"{server.Id} - {server.Name}")
                .WithEmote(Emote.Parse("<:server3:968614410228736070>"))
                .WithValue(server.Id.ToString()));
        }
        
        int totalPages = (int)Math.Ceiling((double)servers.Count / 25 -1);
        bool lastPage = nextPage == totalPages;
        bool firstPage = nextPage == 0;
        
        components = new ComponentBuilder();
        components.WithSelectMenu(
            "ServerSelectorList",
            selectOptions,
            "Select the server you want to edit.");
        
        components.WithButton("Panel",
            emote: Emote.Parse("<a:Earth:1092814004113657927>"),
            style: ButtonStyle.Link,
            url: $"{ConfigService.GetSection("Moonlight").GetValue<string>("AppUrl")}");
        
        components.WithButton("Previous-page",
            emote: Emote.Parse("<:ArrowLeft:1101547474180649030>"),
            style: ButtonStyle.Secondary,
            customId:$"SmPreviousPage.{nextPage}",
            disabled: firstPage);
        
        components.WithButton("Next-page",
            emote: Emote.Parse("<:ArrowRight:1101547475380228257>"),
            style: ButtonStyle.Secondary, 
            customId:$"SmNextPage.{nextPage}",
            disabled: lastPage);

        await component.DeferAsync();
        await component.ModifyOriginalResponseAsync(x => x.Components = components.Build());
    }

    private async Task MenuHandler(SocketMessageComponent component)
    {
        if (component.Data.CustomId != "ServerSelectorList") return;
        
        var dcs = Scope.ServiceProvider.GetRequiredService<DiscordBotService>();
        if (!int.TryParse(component.Data.Values.FirstOrDefault(), out int serverId))
        {
            await ErrorEmbedSnippet(component);
            return;
        }

        var serverRepo = Scope.ServiceProvider.GetRequiredService<ServerRepository>();
        var server = serverRepo.Get()
            .Include(x => x.Owner)
            .Include(x => x.Node)
            .Include(x => x.MainAllocation)
            .FirstOrDefault(x => x.Id == serverId);

        if (server == null || server.Owner.DiscordId != component.User.Id)
        {
            await ErrorEmbedSnippet(component);
            return;
        }

        var embed = dcs.EmbedBuilderModule.ServerManagerEmbed("You're server", Color.Blue, component.User, server);

        var components = new ComponentBuilder();

        if (ConfigService.GetSection("Moonlight").GetSection("DiscordBot").GetValue<bool>("PowerActions"))
        {
            components.WithButton("Start", style: ButtonStyle.Success, customId: $"Sm.Start.{server.Id}", disabled: false);
            components.WithButton("Restart", style: ButtonStyle.Primary, customId: $"Sm.Restart.{server.Id}", disabled: false);
            components.WithButton("Stop", style: ButtonStyle.Danger, customId: $"Sm.Stop.{server.Id}", disabled: false);
            components.WithButton("Kill", style: ButtonStyle.Danger, customId: $"Sm.Kill.{server.Id}", disabled: false);
        }

        components.WithButton("Way2Server",
            emote: Emote.Parse("<a:Earth:1092814004113657927>"),
            style: ButtonStyle.Link,
            url: $"{ConfigService.GetSection("Moonlight").GetValue<string>("AppUrl")}/server/{server.Uuid}");

        components.WithButton("Update",
            emote: Emote.Parse("<:refresh:1101547898803605605>"),
            style: ButtonStyle.Secondary,
            customId: $"Sm.Update.{server.Id}");
        
        if (ConfigService.GetSection("Moonlight").GetSection("DiscordBot").GetValue<bool>("SendCommands"))
        {
            components.WithButton("SendCommand",
                emote: Emote.Parse("<:Console:1101547358157819944>"),
                style: ButtonStyle.Secondary,
                customId: $"Sm.SendCommand.{server.Id}");
        }

        await component.DeferAsync();
        await component.ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
        await component.ModifyOriginalResponseAsync(x => x.Components = components.Build());
    }

    private async Task ErrorEmbedSnippet(SocketMessageComponent component)
    {
        var dcs = Scope.ServiceProvider.GetRequiredService<DiscordBotService>();
        
        var embed = dcs.EmbedBuilderModule.StandardEmbed("Sorry :( \n Something went wrong. \n Please try again later.", Color.Red, component.User);
        await component.RespondAsync(embed: embed.Build(), ephemeral: true);
    }
}