using Microsoft.AspNetCore.Components;

namespace Moonlight.Client.App.Interfaces;

public interface IAdminDashboardCard
{
    public string RequiredPermission { get; }
    public int Columns { get; }

    public Task<RenderFragment> Render();
}