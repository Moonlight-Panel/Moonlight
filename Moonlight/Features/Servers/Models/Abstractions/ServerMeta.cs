using MoonCore.Helpers;
using Moonlight.Features.Servers.Models.Enums;

namespace Moonlight.Features.Servers.Models.Abstractions;

public class ServerMeta
{
    public ServerState State { get; set; }
    public DateTime LastChangeTimestamp { get; set; } = DateTime.UtcNow;
    public SmartEventHandler OnStateChanged { get; set; } = new();
    public SmartEventHandler<string> OnConsoleMessage { get; set; } = new();
    public List<string> ConsoleMessages { get; set; } = new();
}