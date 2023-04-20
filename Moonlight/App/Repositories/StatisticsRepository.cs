using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database;
using Moonlight.App.Database.Entities;
using Moonlight.App.Services;

namespace Moonlight.App.Repositories;

public class StatisticsRepository : IDisposable
{
    private readonly DataContext DataContext;
    private readonly DateTimeService DateTimeService;

    public StatisticsRepository(DataContext dataContext, DateTimeService dateTimeService)
    {
        DataContext = dataContext;
        DateTimeService = dateTimeService;
    }

    public DbSet<StatisticsData> Get()
    {
        return DataContext.Statistics;
    }

    public StatisticsData Add(StatisticsData data)
    {
        var x = DataContext.Statistics.Add(data);
        DataContext.SaveChanges();
        return x.Entity;
    }

    public StatisticsData Add(string chart, double value)
    {
        return Add(new StatisticsData() {Chart = chart, Value = value, Date = DateTimeService.GetCurrent()});
    }

    public void Dispose()
    {
        DataContext.Dispose();
    }
}