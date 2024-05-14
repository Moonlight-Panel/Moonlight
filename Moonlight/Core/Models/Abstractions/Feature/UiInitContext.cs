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
    public List<AdminComponent> AdminPageComponents { get; set; } = new();
    public List<AdminComponent> AdminPageCards { get; set; } = new();
    
    public void EnablePages<T>()
    {
        var assembly = typeof(T).Assembly;
        
        if(!RouteAssemblies.Contains(assembly))
            RouteAssemblies.Add(assembly);
    }

    public void AddAdminComponent<T>(int index = 0, int requiredPermissionLevel = 0) where T : IComponent
    {
        // Loads the Component into a List of AdminComponents, with lots of more information for the Admin Page to render
        AdminPageComponents.Add(
            new AdminComponent()
            {
                Component = builder =>
                {
                    builder.OpenComponent<T>(0);
                    builder.CloseComponent();
                },
                Index = index,
                RequiredPermissionLevel = requiredPermissionLevel
            }
        );
    }
    
    public void AddAdminCard<T>(int index = 0, int requiredPermissionLevel = 0) where T : IComponent
    {
        // Loads the Card into a List of AdminComponents, with lots of more information for the Admin Page to render
        AdminPageCards.Add(
            new AdminComponent()
            {
                Component = builder =>
                {
                    builder.OpenComponent<T>(0);
                    builder.CloseComponent();
                },
                Index = index,
                RequiredPermissionLevel = requiredPermissionLevel
            }
        );
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