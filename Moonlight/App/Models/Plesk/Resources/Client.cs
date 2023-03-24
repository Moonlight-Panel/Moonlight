using Newtonsoft.Json;

namespace Moonlight.App.Models.Plesk.Resources;

public class Client
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("created")]
    public DateTimeOffset Created { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("company")]
    public string Company { get; set; }

    [JsonProperty("login")]
    public string Login { get; set; }

    [JsonProperty("status")]
    public long Status { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("locale")]
    public string Locale { get; set; }

    [JsonProperty("guid")]
    public Guid Guid { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }
}