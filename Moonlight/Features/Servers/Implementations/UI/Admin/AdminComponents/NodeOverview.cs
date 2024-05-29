﻿using MoonCoreUI.Helpers;
using Moonlight.Core.Interfaces.Ui.Admin;
using Moonlight.Core.Models.Abstractions;
using Moonlight.Features.Servers.UI.Components.Cards;

namespace Moonlight.Features.Servers.Implementations.UI.Admin.AdminComponents;

public class NodeOverview : IAdminDashboardComponent
{
    public Task<UiComponent> Get()
    {
        var res = new UiComponent()
        {
            Component = ComponentHelper.FromType<AdminNodesComponent>(),
            RequiredPermissionLevel = 5001
        };

        return Task.FromResult(res);
    }
}