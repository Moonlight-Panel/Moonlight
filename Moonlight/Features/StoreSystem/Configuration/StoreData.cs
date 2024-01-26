using System.ComponentModel;
using Newtonsoft.Json;

namespace Moonlight.Features.StoreSystem.Configuration;

public class StoreData
{
    [JsonProperty("Currency")]
    [Description("A string value representing the currency which will be shown to a user")]
    public string Currency { get; set; } = "€";
}