using Discord;
using Discord.WebSocket;
using Logging.Net;

namespace Moonlight.App.Services.DiscordBot;

public class DiscordBotService
{
    private readonly IServiceScopeFactory ServiceScopeFactory;
    private readonly ConfigService ConfigService;
    
    private IServiceScope ServiceScope;

    private readonly DiscordSocketClient Client;
    
    // Add here references so e.g.
    // public ExampleModule ExampleModule { get; private set; }

    public DiscordBotService(
        IServiceScopeFactory serviceScopeFactory,
        ConfigService configService)
    {
        ServiceScopeFactory = serviceScopeFactory;
        ConfigService = configService;
        Client = new();

        Task.Run(MainAsync);
    }

    private async Task MainAsync()
    {
        ServiceScope = ServiceScopeFactory.CreateScope();

        var discordConfig = ConfigService
            .GetSection("Moonlight")
            .GetSection("DiscordBot");
        
        if(!discordConfig.GetValue<bool>("Enable"))
            return;
        
        Client.Log += Log;
        
        // Init here the modules e.g.
        // ExampleModule = new(Client, ConfigService, Scope)

        await Client.LoginAsync(TokenType.Bot, discordConfig.GetValue<string>("Token"));
        await Client.StartAsync();

        await Task.Delay(-1);
    }

    private Task Log(LogMessage message)
    {
        Logger.Debug(message.Message);
        return Task.CompletedTask;
    }
}