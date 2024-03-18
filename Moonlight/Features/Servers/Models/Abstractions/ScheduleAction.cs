using Moonlight.Features.Servers.Entities;

namespace Moonlight.Features.Servers.Models.Abstractions;

public abstract class ScheduleAction
{
    public string DisplayName { get; set; }
    public string Icon { get; set; }
    public Type FormType { get; set; }

    public abstract Task Execute(Server server, object config, IServiceProvider serviceProvider);
}