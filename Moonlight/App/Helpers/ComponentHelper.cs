using Microsoft.AspNetCore.Components;

namespace Moonlight.App.Helpers;

public static class ComponentHelper
{
    public static RenderFragment FromType(Type type, Action<Dictionary<string, object>>? buildAttributes = null) => builder =>
    {
        builder.OpenComponent(0, type);

        if (buildAttributes != null)
        {
            Dictionary<string, object> parameters = new();
            buildAttributes.Invoke(parameters);
            builder.AddMultipleAttributes(1, parameters);
        }
        
        builder.CloseComponent();
    };
}