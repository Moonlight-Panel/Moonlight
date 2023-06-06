using Newtonsoft.Json;

namespace Moonlight.App.Models.Misc;

public class HealthCheck
{
    public string Status { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public Dictionary<string, HealthCheckEntry> Entries { get; set; } = new();
    
    public class HealthCheckEntry
    {
        public Dictionary<string, string> Data { get; set; } = new();
        public string Description { get; set; }
        public TimeSpan Duration { get; set; }
        public string Status { get; set; }
        public List<string> Tags { get; set; } = new();
    }
}