using Microsoft.AspNetCore.Components;
using MoonCore.Blazor.Helpers;
using Moonlight.Client.App.Interfaces;

namespace Moonlight.Client.App.Implementations.AdminDashboardCards;

public class UserCounterCard : IAdminDashboardCard
{
    public string RequiredPermission { get; } = "admin.users.get";
    public int Columns { get; } = 1;
    public Task<RenderFragment> Render()
    {
        var render = ComponentHelper.FromType<UserCounterComponent>();
        return Task.FromResult(render);
    }
}