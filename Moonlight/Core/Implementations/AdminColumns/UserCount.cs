using Microsoft.AspNetCore.Components;
using MoonCoreUI.Helpers;
using Moonlight.Core.Interfaces;
using Moonlight.Core.UI.Components.Cards;

namespace Moonlight.Core.Implementations.AdminColumns;

public class UserCount : IAdminDashboardColumn
{
    public Task<RenderFragment> Get()
    {
        var res = ComponentHelper.FromType<AdminUserCard>();

        return Task.FromResult(res);
    }
}