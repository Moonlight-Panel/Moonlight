using Microsoft.AspNetCore.Components;
using Moonlight.App.Database.Entities;
using Moonlight.App.Services.Interop;

namespace Moonlight.App.Models.Misc;

public class Session
{
    public string Ip { get; set; } = "N/A";
    public string Url { get; set; } = "N/A";
    public string Device { get; set; } = "N/A";
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; }
    public NavigationManager Navigation { get; set; }
    public AlertService AlertService { get; set; }
}