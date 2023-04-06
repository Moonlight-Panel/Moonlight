using Newtonsoft.Json;

namespace Moonlight.App.Models.Plesk.Requests;

public class CreateDatabaseUser
{
    [JsonProperty("login")]
    public string Login { get; set; }

    [JsonProperty("password")]
    public string Password { get; set; }

    [JsonProperty("database_id")]
    public int DatabaseId { get; set; }
}