using Microsoft.AspNetCore.Components;
using MoonCore.Blazor.Services;
using MoonCore.Services;

namespace Moonlight.Core.Models;

public class Session
{
    public IdentityService IdentityService { get; set; }
    public DateTime CreatedAt { get; set; }
    public NavigationManager NavigationManager { get; set; }
    public AlertService AlertService { get; set; }
    public DateTime UpdatedAt { get; set; }
}