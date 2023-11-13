using System.Diagnostics;
using Discord;
using Discord.WebSocket;
using Moonlight.App.Configuration;
using Moonlight.App.Helpers;
using Moonlight.App.Services.Discord.Bot.Commands;
using Moonlight.App.Services.Discord.Bot.Module;

namespace Moonlight.App.Services.Discord.Bot;


public class DiscordBotService
{
    public static bool Enabled = false;
    
    private ConfigV1.DiscordData.BotData DiscordConfig;
    
    private readonly IServiceScopeFactory ServiceScopeFactory;
    private readonly ConfigService ConfigService;
    
    private IServiceScope ServiceScope;
    
    private readonly DiscordSocketClient Client;
    
    // References
    public CommandControllerModule CommandControllerModule { get; private set; }
    public EmbedBuilderModule EmbedBuilderModule { get; private set; }
    public HelpCommand HelpCommand { get; private set; }
    
    
    public DiscordBotService(IServiceScopeFactory serviceScopeFactory, ConfigService configService)
    {
        ServiceScopeFactory = serviceScopeFactory;
        ConfigService = configService;
        Client = new DiscordSocketClient( new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.All
        });

        Task.Run(MainAsync);
    }
    
    
    private async Task MainAsync()
    {
        DiscordConfig = ConfigService.Get().Discord.Bot;
        if(!DiscordConfig.Enable == false) return;
        
        ServiceScope = ServiceScopeFactory.CreateScope();
        
        Client.Log += Log;
        Client.Ready += OnReady;

        //Commands
        CommandControllerModule = new CommandControllerModule(Client, ConfigService, ServiceScope);
        HelpCommand = new HelpCommand(Client, ConfigService, ServiceScope);
        
        
        //Module
        EmbedBuilderModule = new EmbedBuilderModule(Client, ConfigService, ServiceScope);

        await Client.LoginAsync(TokenType.Bot, DiscordConfig.Token);
        await Client.StartAsync();

        await Task.Delay(-1);
    }
    
    
    private async Task OnReady()
    {
        await Client.SetStatusAsync(UserStatus.Idle);

        Logger.Info($"Invite link: https://discord.com/api/oauth2/authorize?client_id={Client.CurrentUser.Id}&permissions=1099511696391&scope=bot%20applications.commands");
        Logger.Info($"Login as {Client.CurrentUser.Username}#{Client.CurrentUser.DiscriminatorValue}");

        CreateCommands();
    }
    
    
    private Task Log(LogMessage message)
    {
        if (message.Exception is { } exception)
        {
            Logger.Error(exception);
            if (exception.InnerException != null)
            {
                Logger.Error(exception.InnerException);
            }
            return Task.CompletedTask;
        }
        
        Logger.Info(message.Message);
        return Task.CompletedTask;
    }

    #region InitLogic
    public async void CreateCommands()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        var type = this.GetType();
        var properties = type.GetProperties();
        Logger.Debug("Start Initializing Commands");
        foreach (var property in properties)
        {
            if (!property.PropertyType.IsSubclassOf(typeof(BaseModule))) continue;
            var instance = (BaseModule)property.GetValue(this)!;
                
            try
            {
                await instance.Init();
                Logger.Debug($"Registered: '{instance}'");
                await Task.Delay(TimeSpan.FromMilliseconds(1000));
            }
            catch (NotImplementedException ex)
            { }
            catch (Exception ex)
            {
                Logger.Error($"Module Error '{instance}' \n{ex.Message}");
                Logger.Error(ex.InnerException);
            }
        }

        stopwatch.Stop();
        Logger.Debug($"Registered all commands. Done in {stopwatch.ElapsedMilliseconds}ms");
    }
    #endregion

}