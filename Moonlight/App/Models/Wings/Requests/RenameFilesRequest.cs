using Newtonsoft.Json;

namespace Moonlight.App.Models.Wings.Requests;

public class RenameFilesRequest
{
    [JsonProperty("root")]
    public string Root { get; set; }

    [JsonProperty("files")] public RenameFilesData[] Files { get; set; }
}

public class RenameFilesData
{
    [JsonProperty("from")]
    public string From { get; set; }

    [JsonProperty("to")]
    public string To { get; set; }
}