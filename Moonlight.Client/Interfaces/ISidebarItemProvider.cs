using Moonlight.Client.Models;

namespace Moonlight.Client.Interfaces;

public interface ISidebarItemProvider
{
    public SidebarItem[] Get();
}