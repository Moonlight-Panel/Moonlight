using Microsoft.AspNetCore.Components;

namespace Moonlight.Core.Models.Abstractions.Feature;

public class AdminComponent
{
    public RenderFragment Component { get; set; }
    
    public int Index { get; set; }
    
    public int RequiredPermissionLevel { get; set; }
}