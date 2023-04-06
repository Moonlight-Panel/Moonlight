using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database;
using Moonlight.App.Database.Entities;

namespace Moonlight.App.Repositories;

public class StatisticsRepository : IDisposable
{
    private readonly DataContext DataContext;

    public StatisticsRepository(DataContext dataContext)
    {
        DataContext = dataContext;
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
        return Add(new StatisticsData() {Chart = chart, Value = value});
    }

    public void Dispose()
    {
        DataContext.Dispose();
    }
}