using Moonlight.Shared.Misc;

namespace Moonlight.Shared.Http.Responses.Admin;

public class ThemeResponse
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