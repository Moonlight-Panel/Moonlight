namespace Moonlight.Features.Servers.Entities;

public class ServerSchedule
{
    public int Id { get; set; }

    public string Name { get; set; } = "";
    public DateTime LastRun { get; set; } = DateTime.MinValue;
    public int ExecutionSeconds { get; set; } = 0;

    public List<ServerScheduleItem> Items { get; set; } = new();
}