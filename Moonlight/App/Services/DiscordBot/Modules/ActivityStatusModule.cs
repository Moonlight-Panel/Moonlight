using Discord;
using Discord.WebSocket;
using Moonlight.App.Database.Entities;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services.DiscordBot.Modules;

public class ActivityStatusModule : BaseModule
{
    
    private List<LoadingMessage> LoadingMessages;
    
    private readonly PeriodicTimer Timer = new(TimeSpan.FromMinutes(1));

    public ActivityStatusModule(DiscordSocketClient client, ConfigService configService, IServiceScope scope) : base(client, configService, scope)
        { }
    public override Task RegisterCommands()
        { return Task.CompletedTask; }
    
    public Task UpdateActivityStatusList()
    {
        var loadingMessageRepo = Scope.ServiceProvider.GetRequiredService<LoadingMessageRepository>();
        LoadingMessages = loadingMessageRepo.Get().ToList();
        
        return Task.CompletedTask;
    }

    public async void ActivityStatusScheduler()
    {
        while (await Timer.WaitForNextTickAsync())
        {
            String random = "https://endelon-hosting.de";
            if (LoadingMessages.Any())
            {
                Random rand = new Random();
                random = LoadingMessages[rand.Next(LoadingMessages.Count)].Message;
            }
            
            await Client.SetGameAsync(random, "https://www.endelon.team", ActivityType.Streaming);
        }
    }

}