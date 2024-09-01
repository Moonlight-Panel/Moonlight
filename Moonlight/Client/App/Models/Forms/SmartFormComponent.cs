using Microsoft.AspNetCore.Components;

namespace Moonlight.Client.App.Models.Forms;

public class SmartFormComponent : ISmartFormItem
{
    public RenderFragment Render { get; set; }
    public int Columns { get; set; }
}