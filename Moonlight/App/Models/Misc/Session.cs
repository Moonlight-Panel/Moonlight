using Microsoft.AspNetCore.Components;
using Moonlight.App.Services.Interop;

namespace Moonlight.App.Models.Misc;

public class Session
{
    public string Ip { get; set; }
    public string Url { get; set; }
    public string Device { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public NavigationManager Navigation { get; set; }
    public AlertService AlertService { get; set; }
}