using Microsoft.AspNetCore.Components;

namespace Moonlight.Core.Interfaces;

public interface IAdminDashboardColumn
{
    public Task<RenderFragment> Get();
}