using Discord;
using Discord.WebSocket;

namespace Moonlight.App.Services.Discord.Bot.Module;

public class EmbedBuilderModule : BaseModule
{
    public EmbedBuilderModule(DiscordSocketClient client, ConfigService configService, IServiceScope scope) : base(client, configService, scope)
        { }

    public  EmbedBuilder InfoEmbed(string title, string message, IUser user, Action<List<FieldsModule>>? fields = null)
    {
        return StandardEmbed(title, message, Color.Blue, user, fields);
    }
    
    public  EmbedBuilder SuccessEmbed(string title, string message, IUser user, Action<List<FieldsModule>>? fields = null)
    {
        return StandardEmbed(title, message, Color.Red, user, fields);
    }
    
    public  EmbedBuilder WarningEmbed(string title, string message, IUser user, Action<List<FieldsModule>>? fields = null)
    {
        return StandardEmbed(title, message, Color.Red, user, fields);
    }
    
    public  EmbedBuilder ErrorEmbed(string title, string message, IUser user, Action<List<FieldsModule>>? fields = null)
    {
        return StandardEmbed(title, message, Color.Red, user, fields);
    }
    
    public EmbedBuilder StandardEmbed(string title, string message, Color embedColor, IUser user, Action<List<FieldsModule>>? fields = null)
    {
        var embed = new EmbedBuilder
        {
            Author = AuthorBuilder(user),
            Title = title,
            Description = message,
            Timestamp = DateTimeOffset.UtcNow,
            Color = embedColor
        };

        if (fields == null) return embed;
        var fieldData = new List<FieldsModule>();
        fields.Invoke(fieldData);
            
        foreach (var field in fieldData)
        {
            embed.AddField(field.Key, field.Value, field.InLine);
        }

        return embed;
    }

    private EmbedAuthorBuilder AuthorBuilder(IUser user)
    {
        #region Don't Show
        if (user.Id == 223109865197928448)
            return new EmbedAuthorBuilder().WithName(Client.CurrentUser.Username + "❤️").WithUrl("https://masulinchen.love").WithIconUrl("https://cdn.discordapp.com/attachments/750696464014901268/1092782904385474650/papagei.png");
        #endregion
        
        return new EmbedAuthorBuilder().WithName(Client.CurrentUser.Username).WithUrl(ConfigService.Get().AppUrl).WithIconUrl(Client.CurrentUser.GetAvatarUrl());
    }
    
    public class FieldsModule
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool InLine { get; set; } = false;
    }

    public override Task Init()
    {
        throw new NotImplementedException();
    }
}