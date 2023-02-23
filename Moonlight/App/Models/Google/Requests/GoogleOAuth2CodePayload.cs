using Newtonsoft.Json;

namespace Moonlight.App.Models.Google.Requests;

public class GoogleOAuth2CodePayload
{
    [JsonProperty("grant_type")]
    public string GrantType { get; set; } = "authorization_code";
    
    [JsonProperty("code")]
    public string Code { get; set; }
    
    [JsonProperty("client_id")]
    public string ClientId { get; set; }
    
    [JsonProperty("client_secret")]
    public string ClientSecret { get; set; }
    
    [JsonProperty("redirect_uri")]
    public string RedirectUri { get; set; }
}