namespace Moonlight.App.ApiClients.Daemon.Resources;

public class MemoryStats
{
    public List<MemoryStick> Sticks { get; set; } = new();
    public double Free { get; set; }
    public double Used { get; set; }
    public double Total { get; set; }
    
    public class MemoryStick
    {
        public int Size { get; set; }
        public string Type { get; set; } = "";
    }
}