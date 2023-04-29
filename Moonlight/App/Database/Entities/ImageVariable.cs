using Newtonsoft.Json;

namespace Moonlight.App.Database.Entities;

public class ImageVariable
{
    [JsonIgnore]
    public int Id { get; set; }
    public string Key { get; set; } = "";
    public string DefaultValue { get; set; } = "";
}