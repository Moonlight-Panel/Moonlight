﻿using Moonlight.App.Database.Entities;
using Moonlight.App.Database.Entities.Store;
using Moonlight.App.Models.Abstractions;

namespace Moonlight.App.Plugins.Contexts;

public class PluginContext
{
    public IServiceCollection Services { get; set; }
    public IServiceProvider Provider { get; set; }
    public IServiceScope Scope { get; set; }
    public WebApplicationBuilder WebApplicationBuilder { get; set; }
    public WebApplication WebApplication { get; set; }
    public List<Action> PreInitTasks = new();
    public List<Action> PostInitTasks = new();
    public Action<List<ServiceUiPage>, ServiceManageContext>? BuildServiceUiPages = null;
}