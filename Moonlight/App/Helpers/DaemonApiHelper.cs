using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions;
using Newtonsoft.Json;
using RestSharp;

namespace Moonlight.App.Helpers;

public class DaemonApiHelper
{
    private readonly RestClient Client;

    public DaemonApiHelper()
    {
        Client = new();
    }
    
    private string GetApiUrl(Node node)
    {
        if(node.Ssl)
            return $"https://{node.Fqdn}:{node.MoonlightDaemonPort}/";
        else
            return $"http://{node.Fqdn}:{node.MoonlightDaemonPort}/";
        //return $"https://{node.Fqdn}:{node.HttpPort}/";
    }
    
    public async Task<T> Get<T>(Node node, string resource)
    {
        RestRequest request = new(GetApiUrl(node) + resource);

        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Accept", "application/json");
        request.AddHeader("Authorization", node.Token);

        var response = await Client.GetAsync(request);

        if (!response.IsSuccessful)
        {
            if (response.StatusCode != 0)
            {
                throw new DaemonException(
                    $"An error occured: ({response.StatusCode}) {response.Content}",
                    (int)response.StatusCode
                );
            }
            else
            {
                throw new Exception($"An internal error occured: {response.ErrorMessage}");
            }
        }

        return JsonConvert.DeserializeObject<T>(response.Content!)!;
    }
}