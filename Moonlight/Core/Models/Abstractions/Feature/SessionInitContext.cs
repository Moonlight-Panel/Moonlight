using MoonCore.Blazor.Components;

namespace Moonlight.Core.Models.Abstractions.Feature;

public class SessionInitContext
{
    public IServiceProvider ServiceProvider { get; set; }
    public LazyLoader LazyLoader { get; set; }
}