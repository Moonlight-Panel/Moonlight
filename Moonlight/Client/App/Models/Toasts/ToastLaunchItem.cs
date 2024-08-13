using Microsoft.AspNetCore.Components;

namespace Moonlight.Client.App.Models.Toasts;

public class ToastLaunchItem
{
    public string Id { get; set; }
    public RenderFragment Render { get; set; }
}