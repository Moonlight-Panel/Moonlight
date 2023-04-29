using Moonlight.App.Database.Entities;
using Moonlight.App.Models.Misc;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services.Statistics;

public class StatisticsViewService
{
    private readonly StatisticsRepository StatisticsRepository;
    private readonly DateTimeService DateTimeService;
    
    public StatisticsViewService(StatisticsRepository statisticsRepository, DateTimeService dateTimeService)
    {
        StatisticsRepository = statisticsRepository;
        DateTimeService = dateTimeService;
    }

    public StatisticsData[] GetData(string chart, StatisticsTimeSpan timeSpan)
    {
        var startDate = DateTimeService.GetCurrent() - TimeSpan.FromHours((int)timeSpan);

        var objs = StatisticsRepository.Get().Where(x => x.Date > startDate && x.Chart == chart);

        return objs.ToArray();
    }
}