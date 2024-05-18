using Microsoft.AspNetCore.Components;

namespace Moonlight.Core.Interfaces;

public interface IAdminDashboardComponent
{
    public Task<RenderFragment> Get();
}