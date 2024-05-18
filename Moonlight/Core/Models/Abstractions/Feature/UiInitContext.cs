using System.ComponentModel;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Moonlight.Core.UI.Components.Partials;
using IComponent = Microsoft.AspNetCore.Components.IComponent;

namespace Moonlight.Core.Models.Abstractions.Feature;

public class UiInitContext
{
    public List<SidebarItem> SidebarItems { get; set; } = new();
    public List<Assembly> RouteAssemblies { get; set; } = new();
    
    public void EnablePages<T>()
    {
        var assembly = typeof(T).Assembly;
        
        if(!RouteAssemblies.Contains(assembly))
            RouteAssemblies.Add(assembly);
    }

    public void AddSidebarItem(string name, string icon, string target, bool isAdmin = false, bool needsExactMatch = false, int index = 0)
    {
        SidebarItems.Add(new SidebarItem()
        {
            Name = name,
            Icon = icon,
            Target = target,
            IsAdmin = isAdmin,
            NeedsExactMath = needsExactMatch,
            Index = index
        });
    }
}