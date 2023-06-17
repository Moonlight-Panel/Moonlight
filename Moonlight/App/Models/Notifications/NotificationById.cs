using Newtonsoft.Json;

namespace Moonlight.App.Models.Notifications;

public class NotificationById : BasicWSModel
{
    [JsonProperty("notification")]
    public int Notification { get; set; }
}