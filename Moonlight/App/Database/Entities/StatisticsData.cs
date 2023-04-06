namespace Moonlight.App.Database.Entities;

public class StatisticsData
{
    public int Id { get; set; }
    
    public string Chart { get; set; }
    
    public double Value { get; set; }
    
    public DateTime Date { get; set; }
}