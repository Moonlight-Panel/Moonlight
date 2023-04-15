using Newtonsoft.Json;

namespace Moonlight.App.Models.Plesk.Resources;

public class CliResult
{
    [JsonProperty("code")]
    public int Code { get; set; }

    [JsonProperty("stdout")] public string Stdout { get; set; } = "";

    [JsonProperty("stderr")] public string Stderr { get; set; } = "";
}