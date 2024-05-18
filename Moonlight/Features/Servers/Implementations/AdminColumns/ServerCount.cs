using Microsoft.AspNetCore.Components;
using MoonCoreUI.Helpers;
using Moonlight.Core.Interfaces;
using Moonlight.Features.Servers.UI.Components.Cards;

namespace Moonlight.Features.Servers.Implementations.AdminColumns;

public class ServerCount : IAdminDashboardColumn
{
    public Task<RenderFragment> Get()
    {
        var res = ComponentHelper.FromType<AdminServersCard>();

        return Task.FromResult(res);
    }
}