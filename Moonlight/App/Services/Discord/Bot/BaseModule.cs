using Discord.WebSocket;
using Moonlight.App.Configuration;

namespace Moonlight.App.Services.Discord.Bot;

public abstract class BaseModule
{
    public DiscordSocketClient Client { get; set; }
    public ConfigService ConfigService { get; set; }
    public IServiceScope Scope { get; set; }
    public ConfigV1.DiscordData.BotData DiscordConfig { get; set; }
    
    public abstract Task Init();
    
    public BaseModule(
        DiscordSocketClient client,
        ConfigService configService,
        IServiceScope scope)
    {
        Client = client;
        ConfigService = configService;
        Scope = scope;

        DiscordConfig = configService.Get().Discord.Bot;
    }
}