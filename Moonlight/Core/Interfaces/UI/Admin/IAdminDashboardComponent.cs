using Moonlight.Core.Models.Abstractions;

namespace Moonlight.Core.Interfaces.Ui.Admin;

public interface IAdminDashboardComponent
{
    public Task<UiComponent> Get();
}