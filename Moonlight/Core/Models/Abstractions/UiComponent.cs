using Microsoft.AspNetCore.Components;

namespace Moonlight.Core.Models.Abstractions;

public class UiComponent
{
    public required RenderFragment Component { get; set; }

    public int Index { get; set; } = 0;

    public int RequiredPermissionLevel { get; set; } = 0;
}