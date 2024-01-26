using Newtonsoft.Json;

namespace Moonlight.Features.Theming.Configuration;

public class ThemeData
{
    [JsonProperty("EnableDefault")] public bool EnableDefault { get; set; } = true;
}