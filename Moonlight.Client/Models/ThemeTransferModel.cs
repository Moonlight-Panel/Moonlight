using Moonlight.Shared.Misc;

namespace Moonlight.Client.Models;

public class ThemeTransferModel
{
    public string Name { get; set; }
    public string Author { get; set; }
    public string Version { get; set; }
    public string? UpdateUrl { get; set; }
    public string? DonateUrl { get; set; }

    public ApplicationTheme Content { get; set; } = new();
}