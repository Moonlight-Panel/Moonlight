using Newtonsoft.Json;

namespace Moonlight.App.Models.Plesk.Resources;

public class DatabaseUser
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("login")]
    public string Login { get; set; }

    [JsonProperty("database_id")]
    public int DatabaseId { get; set; }
}