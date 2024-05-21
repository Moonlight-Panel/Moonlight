using Moonlight.Core.Models.Abstractions;

namespace Moonlight.Core.Interfaces.Ui.Admin;

public interface IAdminDashboardColumn
{
    public Task<UiComponent> Get();
}