using Microsoft.AspNetCore.Components;
using Moonlight.App.Database.Entities;
using Moonlight.App.Database.Entities.Store;
using Moonlight.App.Helpers;

namespace Moonlight.App.Models.Abstractions.Services;

public class ServiceViewContext
{
    // Meta
    public Service Service { get; set; }
    public User User { get; set; }
    public Product Product { get; set; }

    // Content
    public List<ServiceUiPage> Pages { get; set; } = new();
    public RenderFragment Layout { get; set; }

    public Task AddPage<T>(string name, string route, string icon = "") where T : ComponentBase
    {
        Pages.Add(new()
        {
            Name = name,
            Route = route,
            Icon = icon,
            Component = ComponentHelper.FromType<T>()
        });
        
        return Task.CompletedTask;
    }
}