using Moonlight.App.Database.Entities;
using Newtonsoft.Json;
using RestSharp;

namespace Moonlight.App.ApiClients.Daemon;

public class DaemonApiHelper
{
    private readonly RestClient Client;

    public DaemonApiHelper()
    {
        Client = new();
    }
    
    public async Task<T> Get<T>(Node node, string resource)
    {
        var request = await CreateRequest(node, resource);

        request.Method = Method.Get;
        
        var response = await Client.ExecuteAsync(request);

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
    
    public async Task Post(Node node, string resource, object body)
    {
        var request = await CreateRequest(node, resource);

        request.Method = Method.Post;

        request.AddParameter("text/plain", JsonConvert.SerializeObject(body), ParameterType.RequestBody);

        var response = await Client.ExecuteAsync(request);

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
    }
    
    public async Task Delete(Node node, string resource, object body)
    {
        var request = await CreateRequest(node, resource);

        request.Method = Method.Delete;

        request.AddParameter("text/plain", JsonConvert.SerializeObject(body), ParameterType.RequestBody);

        var response = await Client.ExecuteAsync(request);

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
    }

    private Task<RestRequest> CreateRequest(Node node, string resource)
    {
        var url = $"http://{node.Fqdn}:{node.MoonlightDaemonPort}/";
        
        RestRequest request = new(url + resource);

        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Accept", "application/json");
        request.AddHeader("Authorization", node.Token);
        
        return Task.FromResult(request);
    }
}