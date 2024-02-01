
using Moonlight.Features.Servers.UI.Layouts;
using Moonlight.Features.ServiceManagement.Models.Abstractions;

namespace Moonlight.Features.Servers.Actions;

public class ServerServiceDefinition : ServiceDefinition
{
    public override ServiceActions Actions => new ServerActions();
    public override Type ConfigType => typeof(ServerConfig);
    public override async Task BuildUserView(ServiceViewContext context)
    {
        context.Layout = typeof(UserLayout);
    }

    public override Task BuildAdminView(ServiceViewContext context)
    {
        throw new NotImplementedException();
    }
}