using Microsoft.AspNetCore.Components;

namespace Moonlight.App.Helpers;

public static class ComponentHelper
{
    public static RenderFragment FromType(Type type) => builder =>
    {
        builder.OpenComponent(0, type);
        builder.CloseComponent();
    };
}