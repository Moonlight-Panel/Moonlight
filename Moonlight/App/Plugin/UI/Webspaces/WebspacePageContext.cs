using Moonlight.App.Database.Entities;

namespace Moonlight.App.Plugin.UI.Webspaces;

public class WebspacePageContext
{
    public List<WebspaceTab> Tabs { get; set; } = new();
    public User User { get; set; }
    public WebSpace WebSpace { get; set; }
}