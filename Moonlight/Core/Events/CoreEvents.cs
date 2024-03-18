using MoonCore.Helpers;

namespace Moonlight.Core.Events;

public class CoreEvents
{
    public static SmartEventHandler OnMoonlightRestart { get; set; } = new();
}