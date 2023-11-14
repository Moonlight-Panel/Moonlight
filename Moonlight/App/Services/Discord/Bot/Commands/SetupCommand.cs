using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Moonlight.App.Services.Discord.Bot.Commands;

public class SetupCommand : BaseModule
{
    public SetupCommand(DiscordSocketClient client, ConfigService configService, IServiceScope scope) : base(client, configService, scope)
        { }
    
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task Handler(SocketSlashCommand command)
    {
        var dsc = Scope.ServiceProvider.GetRequiredService<DiscordBotService>();
        var guild = Client.GetGuild((ulong)DiscordConfig.GuildId);
        var user = guild.GetUser(command.User.Id);
        
        // Permission check
        if (user is { GuildPermissions.Administrator: false } || guild.CurrentUser.GuildPermissions is { ManageChannels: false })
        {
            await command.RespondAsync(ephemeral: true, embed: dsc.EmbedBuilderModule.ErrorEmbed("Insufficient permissions", "You must have Administrator \n The Bot must have `Channel Edit` Permissions", command.User).Build());
            return;
        }
        
        // Already setup
        if (guild.GetChannel((ulong)DiscordConfig.PostChannelId) != null)
        {
            await command.RespondAsync(ephemeral: true, embed: dsc.EmbedBuilderModule.ErrorEmbed("Already setup", "Setup canceled!", command.User).Build());
            return;
        }
        
        // Automatic setup
        if (command.Data.Options.FirstOrDefault() != null && (bool)command.Data.Options.FirstOrDefault())
        {
            await command.RespondAsync(ephemeral: true, embed: dsc.EmbedBuilderModule.WarningEmbed("Automatic Setup", "This is the fast setup.\n Please wait...", command.User).Build());
            
            var postChannel = await guild.CreateForumChannelAsync("NotifyChannel", x =>
            {
                x.DefaultLayout = ForumLayout.List;
                x.Flags = ChannelFlags.Pinned;
                x.PermissionOverwrites = new List<Overwrite>
                {
                    new(guild.EveryoneRole.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Deny)),
                    new(guild.CurrentUser.Id, PermissionTarget.User, new OverwritePermissions(viewChannel: PermValue.Allow))
                };
            });
            
            var posts = new List<long>();
            
            foreach (DiscordLogging x in Enum.GetValues(typeof(DiscordLogging)))
            {
                var post = await postChannel.CreatePostAsync( $"{x}", ThreadArchiveDuration.OneWeek,
                    embed: dsc.EmbedBuilderModule.InfoEmbed($"{x}", "# Don't Remove the Channel \n Logging for ...", Client.CurrentUser).Build());
                
                posts.Add((long)post.Id);
            }
            
            //Save to Config
            ConfigService.Get().Discord.Bot.PostChannelId = (long)postChannel.Id;
            ConfigService.Get().Discord.Bot.PostIds = posts;
            ConfigService.Save();
            
            return;
        }
        
        await command.RespondAsync(ephemeral: true, embed: dsc.EmbedBuilderModule.SuccessEmbed("Manuel Setup", "This is the custom setup.", command.User).Build());


    }
    
    public override async Task Init()
    {
        var command = new SlashCommandBuilder()
        {
            Name = "setup",
            Description = "Setup the bot and Channels",
            DefaultMemberPermissions = GuildPermission.ViewAuditLog,
            Options = new List<SlashCommandOptionBuilder>(new List<SlashCommandOptionBuilder>
            {
                new()
                {
                    Name = "fast",
                    Description = "Fast Setup channel will be automatically created",
                    Type = ApplicationCommandOptionType.Boolean,
                    IsRequired = false
                }
            })
        };
        
        await Client.GetGuild((ulong)DiscordConfig.GuildId).CreateApplicationCommandAsync(command.Build());
    }
}