using Moonlight.App.Database;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services.Statistics;

public class StatisticsCaptureService
{
    private readonly DataContext DataContext;
    private readonly ConfigService ConfigService;
    private readonly StatisticsRepository StatisticsRepository;
    private readonly IServiceScopeFactory ServiceScopeFactory;
    private readonly WebSpaceService WebSpaceService;
    private readonly PeriodicTimer Timer;
    
    public StatisticsCaptureService(IServiceScopeFactory serviceScopeFactory, ConfigService configService)
    {
        ServiceScopeFactory = serviceScopeFactory;
        var provider = ServiceScopeFactory.CreateScope().ServiceProvider;
            
        DataContext = provider.GetRequiredService<DataContext>();
        ConfigService = configService;
        StatisticsRepository = provider.GetRequiredService<StatisticsRepository>();
        WebSpaceService = provider.GetRequiredService<WebSpaceService>();

        var config = ConfigService.GetSection("Moonlight").GetSection("Statistics");
        if(!config.GetValue<bool>("Enabled"))
            return;

        var _period = config.GetValue<int>("Wait");
        var period = TimeSpan.FromMinutes(_period);
        Timer = new(period);

        Task.Run(Run);
    }

    private async Task Run()
    {
        while (await Timer.WaitForNextTickAsync())
        {
            StatisticsRepository.Add("statistics.usersCount", DataContext.Users.Count());
            StatisticsRepository.Add("statistics.serversCount", DataContext.Servers.Count());
            StatisticsRepository.Add("statistics.domainsCount", DataContext.Domains.Count());
            StatisticsRepository.Add("statistics.webspacesCount", DataContext.WebSpaces.Count());
            StatisticsRepository.Add("statistics.databasesCount", DataContext.Databases.Count());
        }
    }
}