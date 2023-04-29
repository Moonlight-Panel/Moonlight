using Moonlight.App.Database;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services.Statistics;

public class StatisticsCaptureService
{
    private readonly DataContext DataContext;
    private readonly ConfigService ConfigService;
    private readonly StatisticsRepository StatisticsRepository;
    private readonly IServiceScopeFactory ServiceScopeFactory;
    private readonly WebsiteService WebsiteService;
    private readonly PleskServerRepository PleskServerRepository;
    private PeriodicTimer Timer;
    
    public StatisticsCaptureService(IServiceScopeFactory serviceScopeFactory, ConfigService configService)
    {
        ServiceScopeFactory = serviceScopeFactory;
        var provider = ServiceScopeFactory.CreateScope().ServiceProvider;
            
        DataContext = provider.GetRequiredService<DataContext>();
        ConfigService = configService;
        StatisticsRepository = provider.GetRequiredService<StatisticsRepository>();
        WebsiteService = provider.GetRequiredService<WebsiteService>();
        PleskServerRepository = provider.GetRequiredService<PleskServerRepository>();

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
            StatisticsRepository.Add("statistics.websitesCount", DataContext.Websites.Count());
            
            int databases = 0;
            
            await foreach (var pleskServer in PleskServerRepository.Get())
            {
                databases += (await WebsiteService.GetDefaultDatabaseServer(pleskServer)).DbCount;
            }
            
            StatisticsRepository.Add("statistics.databasesCount", databases);
        }
    }
}