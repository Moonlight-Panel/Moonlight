using Discord;
using Discord.WebSocket;
using Moonlight.App.Database.Entities;

namespace Moonlight.App.Services.DiscordBot.Modules;

public class EmbedBuilderModule : BaseModule
{

    public EmbedBuilderModule(DiscordSocketClient client, ConfigService configService, IServiceScope scope) : base(client, configService, scope)
        { }
    public override Task RegisterCommands()
    {
        return Task.CompletedTask;
    }

    public EmbedBuilder StandardEmbed(string message, Color embedColor, IUser user, Dictionary<string, string>? fields = null)
    {
        var embed = new EmbedBuilder
        {
            Author = AThing(user),
            Description = message,
            Timestamp = DateTimeOffset.UtcNow,
            Color = embedColor
        };

        if (fields != null)
        {
            foreach (var field in fields)
            {
                embed.AddField(field.Key, field.Value);
            }
        }

        return embed;
    }
    
    public EmbedBuilder ServerManagerEmbed(string message, Color embedColor, IUser user, Server server)
    {
        var embed = new EmbedBuilder
        {
            Author = AThing(user),
            Description = message,
            Timestamp = DateTimeOffset.UtcNow,
            Color = embedColor
        };
        
        embed.AddField("Server Name", $"```{server.Id} - {server.Name}```", inline: true);
        embed.AddField("Owner", $"```{server.Owner.FirstName} {server.Owner.LastName}```", inline: true);
        embed.AddField("Node", $"```{server.Node.Name}```", inline: true);
        embed.AddField("Cpu", $"```{server.Cpu.ToString()}```", inline: true);
        embed.AddField("Ram", $"```{server.Memory.ToString()}```", inline: true);
        embed.AddField("Disk", $"```{server.Cpu.ToString()}```", inline: true);
        embed.AddField("\u200b", "\u200b");
        embed.AddField("Address", $"```{server.Node.Fqdn}:{server.MainAllocation.Port.ToString()}```", inline: false);


        return embed;
    }
    
    public EmbedBuilder ColorChangerServerManagerEmbed(Embed? oldEmbed, Color embedColor, IUser user)
    {
        var embed = new EmbedBuilder
        {
            Author = AThing(user),
            Description = oldEmbed.Description,
            Timestamp = DateTimeOffset.UtcNow,
            Color = embedColor
        };

        foreach (var field in oldEmbed.Fields)
        {
            embed.AddField(field.Name, field.Value, field.Inline);
        }
        
        return embed;
    }

    private EmbedAuthorBuilder AThing(IUser user)
    {
        #region Don't Show
        if (user.Id == 223109865197928448)
            return new EmbedAuthorBuilder().WithName(Client.CurrentUser.Username + "❤️").WithUrl("https://masulinchen.love").WithIconUrl("https://cdn.discordapp.com/attachments/750696464014901268/1092782904385474650/papagei.png");
        #endregion
        
        Random random = new Random();
        int[] randomNumbers = new int[] { 1, 3, 8, 11, 20 };
        
        if (randomNumbers.Contains(random.Next(1, 24)))
            return new EmbedAuthorBuilder().WithName(Client.CurrentUser.Username + " - The Rick version").WithUrl(ConfigService.Get().Moonlight.AppUrl).WithIconUrl("https://cdn.discordapp.com/attachments/750696464014901268/1092783310129860618/rick.gif");
        
        return new EmbedAuthorBuilder().WithName(Client.CurrentUser.Username).WithUrl(ConfigService.Get().Moonlight.AppUrl).WithIconUrl(Client.CurrentUser.GetAvatarUrl());
    }
}
