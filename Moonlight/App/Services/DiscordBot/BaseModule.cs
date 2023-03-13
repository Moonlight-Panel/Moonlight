using Discord.WebSocket;

namespace Moonlight.App.Services.DiscordBot;

public abstract class BaseModule
{
    public DiscordSocketClient Client { get; set; }
    public ConfigService ConfigService { get; set; }
    public IServiceScope Scope { get; set; }
    
    public abstract Task RegisterCommands();
    
    public BaseModule(
        DiscordSocketClient client,
        ConfigService configService,
        IServiceScope scope)
    {
        Client = client;
        ConfigService = configService;
        Scope = scope;
    }
}