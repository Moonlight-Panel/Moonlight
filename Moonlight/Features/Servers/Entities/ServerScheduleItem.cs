namespace Moonlight.Features.Servers.Entities;

public class ServerScheduleItem
{
    public int Id { get; set; }
    
    public string Action { get; set; }
    public string DataJson { get; set; }
    public int Priority { get; set; }
}