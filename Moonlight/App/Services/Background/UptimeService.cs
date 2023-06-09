namespace Moonlight.App.Services.Background;

public class UptimeService
{
    public DateTime StartTimestamp { get; private set; }
    
    public UptimeService()
    {
        StartTimestamp = DateTime.UtcNow;
    }
}