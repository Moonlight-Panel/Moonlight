using Microsoft.AspNetCore.Components;

namespace Moonlight.App.Models.Abstractions;

public class ServiceUiPage
{
    public string Name { get; set; }
    public string Route { get; set; }
    public string Icon { get; set; }
    public ComponentBase Component { get; set; }
}