using Microsoft.AspNetCore.Components;

namespace Moonlight.Client.App.Models.Modals;

public class ModalLaunchItem
{
    public string Id { get; set; }
    public RenderFragment Render { get; set; }
    public string Size { get; set; }
}