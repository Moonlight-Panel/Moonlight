using Newtonsoft.Json;
using RestSharp;

namespace Moonlight.App.ApiClients.Telemetry;

public class TelemetryApiHelper
{
    private readonly RestClient Client;

    public TelemetryApiHelper()
    {
        Client = new();
    }

    public async Task Post(string resource, object? body)
    {
        var request = CreateRequest(resource);

        request.Method = Method.Post;
        
        request.AddParameter("application/json", JsonConvert.SerializeObject(body), ParameterType.RequestBody);

        var response = await Client.ExecuteAsync(request);
        
        if (!response.IsSuccessful)
        {
            if (response.StatusCode != 0)
            {
                throw new TelemetryException(
                    $"An error occured: ({response.StatusCode}) {response.Content}",
                    (int)response.StatusCode
                );
            }
            else
            {
                throw new Exception($"An internal error occured: {response.ErrorMessage}");
            }
        }
    }

    private RestRequest CreateRequest(string resource)
    {
        var url = "https://telemetry.moonlightpanel.xyz/" + resource;

        var request = new RestRequest(url)
        {
            Timeout = 3000000
        };

        return request;
    }
}