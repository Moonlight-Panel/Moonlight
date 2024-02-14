using Moonlight.Features.Servers.Entities.Enums;

namespace Moonlight.Features.Servers.Entities;

public class ServerSchedule
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Cron { get; set; }

    public ScheduleActionType ActionType { get; set; }
    public string ActionData { get; set; } = "";

    public DateTime LastRunAt { get; set; } = DateTime.Now;
    public bool WasLastRunAutomatic { get; set; } = false;
}