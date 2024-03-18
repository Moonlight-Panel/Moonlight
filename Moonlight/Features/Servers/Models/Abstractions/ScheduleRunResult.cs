namespace Moonlight.Features.Servers.Models.Abstractions;

public class ScheduleRunResult
{
    public bool Failed { get; set; }
    public int ExecutionSeconds { get; set; }
}