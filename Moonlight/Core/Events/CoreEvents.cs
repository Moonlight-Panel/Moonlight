using MoonCore.Attributes;
using MoonCore.Helpers;

namespace Moonlight.Core.Events;

[Singleton]
public class CoreEvents
{
    public CoreEvents(ILogger<SmartEventHandler> logger)
    {
        OnMoonlightRestart = new(logger);
    }

    public SmartEventHandler OnMoonlightRestart { get; set; }
}