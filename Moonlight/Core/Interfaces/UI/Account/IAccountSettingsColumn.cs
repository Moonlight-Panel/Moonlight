using Moonlight.Core.Models.Abstractions;

namespace Moonlight.Core.Interfaces.UI.Account;

public interface IAccountSettingsColumn
{
    public Task<UiComponent> Get();
}