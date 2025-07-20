using Moonlight.ApiServer.Models;

namespace Moonlight.ApiServer.Database.Entities;

public class Theme
{
    public int Id { get; set; }

    public bool IsEnabled { get; set; }

    public string Name { get; set; }
    public string Author { get; set; }
    public string Version { get; set; }
    
    public string? UpdateUrl { get; set; }
    public string? DonateUrl { get; set; }

    public ApplicationTheme Content { get; set; }
}