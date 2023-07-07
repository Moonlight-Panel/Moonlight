using Moonlight.App.ApiClients.Telemetry;
using Moonlight.App.ApiClients.Telemetry.Requests;
using Moonlight.App.Database.Entities;
using Moonlight.App.Helpers;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services.Background;

public class TelemetryService
{
    private readonly IServiceScopeFactory ServiceScopeFactory;
    private readonly ConfigService ConfigService;
    
    public TelemetryService(
        ConfigService configService,
        IServiceScopeFactory serviceScopeFactory)
    {
        ServiceScopeFactory = serviceScopeFactory;
        ConfigService = configService;
        
        if(!ConfigService.DebugMode)
            Task.Run(Run);
    }
    
    private async Task Run()
    {
        var timer = new PeriodicTimer(TimeSpan.FromMinutes(15));
        
        while (true)
        {
            using var scope = ServiceScopeFactory.CreateScope();
            
            var serversRepo = scope.ServiceProvider.GetRequiredService<Repository<Server>>();
            var nodesRepo = scope.ServiceProvider.GetRequiredService<Repository<Node>>();
            var usersRepo = scope.ServiceProvider.GetRequiredService<Repository<User>>();
            var webspacesRepo = scope.ServiceProvider.GetRequiredService<Repository<WebSpace>>();
            var databaseRepo = scope.ServiceProvider.GetRequiredService<Repository<MySqlDatabase>>();

            var apiHelper = scope.ServiceProvider.GetRequiredService<TelemetryApiHelper>();

            try
            {
                await apiHelper.Post("telemetry", new TelemetryData()
                {
                    Servers = serversRepo.Get().Count(),
                    Databases = databaseRepo.Get().Count(),
                    Nodes = nodesRepo.Get().Count(),
                    Users = usersRepo.Get().Count(),
                    Webspaces = webspacesRepo.Get().Count(),
                    AppUrl = ConfigService.Get().Moonlight.AppUrl
                });
            }
            catch (Exception e)
            {
                Logger.Warn("Error sending telemetry");
                Logger.Warn(e);
            }
            
            await timer.WaitForNextTickAsync();
        }
    }
}