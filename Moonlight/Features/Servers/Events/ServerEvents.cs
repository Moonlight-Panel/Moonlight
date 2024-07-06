using MoonCore.Attributes;
using MoonCore.Helpers;
using Moonlight.Features.Servers.Entities;

namespace Moonlight.Features.Servers.Events;

[Singleton]
public class ServerEvents
{
    public SmartEventHandler<(Server, ServerBackup)> OnBackupCompleted { get; set; }
    
    public ServerEvents(ILogger<SmartEventHandler> eventHandlerLogger)
    {
        OnBackupCompleted = new(eventHandlerLogger);
    }
}