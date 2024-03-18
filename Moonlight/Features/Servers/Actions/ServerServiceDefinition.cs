using Moonlight.Features.Servers.UI.Layouts;
using Moonlight.Features.Servers.UI.UserViews;
using Moonlight.Features.ServiceManagement.Models.Abstractions;
using Console = Moonlight.Features.Servers.UI.UserViews.Console;

namespace Moonlight.Features.Servers.Actions;

public class ServerServiceDefinition : ServiceDefinition
{
    public override ServiceActions Actions => new ServerActions();
    public override Type ConfigType => typeof(ServerConfig);

    public override async Task BuildUserView(ServiceViewContext context)
    {
        context.Layout = typeof(UserLayout);

        await context.AddPage<Console>("Console", "/", "bx bx-sm bxs-terminal");
        await context.AddPage<Files>("Files", "/files", "bx bx-sm bxs-folder");
        await context.AddPage<Network>("Network", "/network", "bx bx-sm bx-cloud");
        await context.AddPage<Schedules>("Schedules", "/schedules", "bx bx-sm bx-timer");
        await context.AddPage<Variables>("Variables", "/variables", "bx bx-sm bx-slider");
        await context.AddPage<Reset>("Reset", "/reset", "bx bx-sm bx-revision");
    }

    public override Task BuildAdminView(ServiceViewContext context)
    {
        throw new NotImplementedException();
    }
}