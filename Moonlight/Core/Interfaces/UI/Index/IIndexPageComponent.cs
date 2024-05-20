using Moonlight.Core.Models.Abstractions;

namespace Moonlight.Core.Interfaces.UI.Index;

public interface IIndexPageComponent
{
    public Task<UiComponent> Get();
}