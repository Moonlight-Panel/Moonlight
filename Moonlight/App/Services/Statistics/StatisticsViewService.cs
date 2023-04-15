using Moonlight.App.Database.Entities;
using Moonlight.App.Models.Misc;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services.Statistics;

public class StatisticsViewService
{
    private readonly StatisticsRepository StatisticsRepository;
    
    public StatisticsViewService(StatisticsRepository statisticsRepository)
    {
        StatisticsRepository = statisticsRepository;
    }

    public StatisticsData[] GetData(string chart, StatisticsTimeSpan timeSpan)
    {
        var startDate = DateTime.Now - TimeSpan.FromHours((int)timeSpan);

        var objs = StatisticsRepository.Get().Where(x => x.Date > startDate && x.Chart == chart);

        return objs.ToArray();
    }
}