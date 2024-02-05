using System.ComponentModel;
using Newtonsoft.Json;

namespace Moonlight.Features.FileManager.Configuration;

public class FileManagerData
{
    [JsonProperty("MaxFileOpenSize")]
    [Description(
        "This specifies the maximum file size a user will be able to open in the file editor in kilobytes")]
    public int MaxFileOpenSize { get; set; } = 1024 * 5; // 5 MB

    [JsonProperty("OperationTimeout")]
    [Description("This specifies the general timeout for file manager operations. This can but has not to be used by file accesses")]
    public int OperationTimeout { get; set; } = 5;
}