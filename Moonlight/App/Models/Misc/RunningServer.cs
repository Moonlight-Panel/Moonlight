using Moonlight.App.ApiClients.Daemon.Resources;
using Moonlight.App.Database.Entities;

namespace Moonlight.App.Models.Misc;

public class RunningServer
{
    public Server Server { get; set; }
    public ContainerStats.Container Container { get; set; }
}