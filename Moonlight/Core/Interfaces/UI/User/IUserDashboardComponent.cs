using Moonlight.Core.Models.Abstractions;

namespace Moonlight.Core.Interfaces.UI.User;

public interface IUserDashboardComponent
{
    public Task<UiComponent> Get();
}