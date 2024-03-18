using MoonCore.Helpers;
using Moonlight.Features.Servers.Entities;

namespace Moonlight.Features.Servers.Events;

public class ServerEvents
{
    public static SmartEventHandler<(Server, ServerBackup)> OnBackupCompleted { get; set; } = new();
}