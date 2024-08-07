using Moonlight.Client.App.Models;

namespace Moonlight.Client.App.Interfaces;

public interface ISidebarItemProvider
{
    public SidebarItem[] GetItems();
}