﻿using Microsoft.AspNetCore.Components;
using MoonCoreUI.Helpers;
using Moonlight.Core.Database.Entities;

using Moonlight.Features.ServiceManagement.Entities;
using Moonlight.Features.StoreSystem.Entities;

namespace Moonlight.Features.ServiceManagement.Models.Abstractions;

public class ServiceViewContext
{
    // Meta
    public Service Service { get; set; }
    public User User { get; set; }
    public Product Product { get; set; }

    // Content
    public List<ServiceUiPage> Pages { get; set; } = new();
    public Type Layout { get; set; }

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