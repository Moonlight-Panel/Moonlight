using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Moonlight.Features.Servers.Configuration;

public class ServersData
{
    [JsonProperty("Schedules")] public SchedulesData Schedules { get; set; } = new();
    
    public class SchedulesData
    {
        [JsonProperty("Disable")]
        [Description("This flag stops the schedule execution system from starting. Changing this requires a restart")]
        public bool Disable { get; set; } = false;
        
        [JsonProperty("SchedulePerServer")]
        [Description("This specifies the schedules a user can create for a server")]
        public int SchedulePerServer { get; set; } = 10;

        [JsonProperty("CheckDelay")]
        [Description("The delay in seconds between every schedule run")]
        public int CheckDelay { get; set; } = 10;

        [JsonProperty("TimeDrift")]
        [Description(
            "The amount of seconds the planned execution time is allowed to vary to the current time and still be executed")]
        public int TimeDrift { get; set; } = 30;
    }
}