using System.ComponentModel;
using Newtonsoft.Json;

namespace Moonlight.Features.Advertisement.Configuration;

public class AdvertisementData
{
    [JsonProperty("PreventAdBlockers")]
    [Description("This prevents users from using ad blockers while using moonlight. (Note: The detection might not always work)")]
    public bool PreventAdBlockers { get; set; }
}