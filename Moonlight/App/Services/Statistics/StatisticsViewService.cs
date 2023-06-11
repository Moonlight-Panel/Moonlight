using Moonlight.App.Database.Entities;
using Moonlight.App.Models.Misc;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services.Statistics;

public class StatisticsViewService
{
    private readonly StatisticsRepository StatisticsRepository;
    private readonly Repository<User> UserRepository;
    private readonly DateTimeService DateTimeService;
    
    public StatisticsViewService(StatisticsRepository statisticsRepository, DateTimeService dateTimeService, Repository<User> userRepository)
    {
        StatisticsRepository = statisticsRepository;
        DateTimeService = dateTimeService;
        UserRepository = userRepository;
    }

    public StatisticsData[] GetData(string chart, StatisticsTimeSpan timeSpan)
    {
        var startDate = DateTimeService.GetCurrent() - TimeSpan.FromHours((int)timeSpan);

        var objs = StatisticsRepository
            .Get()
            .Where(x => x.Date > startDate && x.Chart == chart);

        return objs.ToArray();
    }

    public int GetActiveUsers(StatisticsTimeSpan timeSpan)
    {
        var startDate = DateTimeService.GetCurrent() - TimeSpan.FromHours((int)timeSpan);

        return UserRepository
            .Get()
            .Count(x => x.LastVisitedAt > startDate);
    }
}