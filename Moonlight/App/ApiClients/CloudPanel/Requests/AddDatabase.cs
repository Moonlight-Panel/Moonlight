using Newtonsoft.Json;

namespace Moonlight.App.ApiClients.CloudPanel.Requests;

public class AddDatabase
{
    [JsonProperty("domainName")]
    public string DomainName { get; set; }

    [JsonProperty("databaseName")]
    public string DatabaseName { get; set; }

    [JsonProperty("databaseUserName")]
    public string DatabaseUserName { get; set; }

    [JsonProperty("databaseUserPassword")]
    public string DatabaseUserPassword { get; set; }
}