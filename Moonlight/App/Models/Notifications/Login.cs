using Newtonsoft.Json;

namespace Moonlight.App.Models.Notifications;

public class Login : BasicWSModel
{
    [JsonProperty("token")] public string Token { get; set; } = "";
}