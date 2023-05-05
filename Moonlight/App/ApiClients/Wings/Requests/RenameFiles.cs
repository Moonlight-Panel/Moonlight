using Newtonsoft.Json;

namespace Moonlight.App.ApiClients.Wings.Requests;

public class RenameFiles
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