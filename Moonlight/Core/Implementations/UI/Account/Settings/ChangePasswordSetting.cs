using MoonCoreUI.Helpers;
using Moonlight.Core.Interfaces.UI.Account;
using Moonlight.Core.Models.Abstractions;
using Moonlight.Core.UI.Components.Partials.Settings;

namespace Moonlight.Core.Implementations.UI.Account.Settings;

public class ChangePasswordSetting : IAccountSettingsColumn
{
    public Task<UiComponent> Get()
    {
        var res = new UiComponent()
        {
            Component = ComponentHelper.FromType<ChangePasswordSettingColumn>()
        };

        return Task.FromResult(res);
    }
}