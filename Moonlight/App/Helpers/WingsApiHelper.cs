using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions;
using Newtonsoft.Json;
using RestSharp;

namespace Moonlight.App.Helpers;

public class WingsApiHelper
{
    private readonly RestClient Client;

    public WingsApiHelper()
    {
        Client = new();
    }

    public async Task<T> Get<T>(Node node, string resource)
    {
        var request = CreateRequest(node, resource);

        request.Method = Method.Get;

        var response = await Client.ExecuteAsync(request);

        if (!response.IsSuccessful)
        {
            if (response.StatusCode != 0)
            {
                throw new WingsException(
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

    public async Task<string> GetRaw(Node node, string resource)
    {
        var request = CreateRequest(node, resource);

        request.Method = Method.Get;

        var response = await Client.ExecuteAsync(request);

        if (!response.IsSuccessful)
        {
            if (response.StatusCode != 0)
            {
                throw new WingsException(
                    $"An error occured: ({response.StatusCode}) {response.Content}",
                    (int)response.StatusCode
                );
            }
            else
            {
                throw new Exception($"An internal error occured: {response.ErrorMessage}");
            }
        }

        return response.Content!;
    }

    public async Task<T> Post<T>(Node node, string resource, object? body)
    {
        var request = CreateRequest(node, resource);

        request.Method = Method.Post;

        request.AddParameter("text/plain",
            JsonConvert.SerializeObject(body),
            ParameterType.RequestBody
        );

        var response = await Client.ExecuteAsync(request);

        if (!response.IsSuccessful)
        {
            if (response.StatusCode != 0)
            {
                throw new WingsException(
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

    public async Task Post(Node node, string resource, object? body)
    {
        var request = CreateRequest(node, resource);

        request.Method = Method.Post;

        if(body != null)
           request.AddParameter("text/plain", JsonConvert.SerializeObject(body), ParameterType.RequestBody);

        var response = await Client.ExecuteAsync(request);

        if (!response.IsSuccessful)
        {
            if (response.StatusCode != 0)
            {
                throw new WingsException(
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

    public async Task PostRaw(Node node, string resource, object body)
    {
        var request = CreateRequest(node, resource);

        request.Method = Method.Post;

        request.AddParameter("text/plain", body, ParameterType.RequestBody);

        var response = await Client.ExecuteAsync(request);

        if (!response.IsSuccessful)
        {
            if (response.StatusCode != 0)
            {
                throw new WingsException(
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

    public async Task Delete(Node node, string resource, object? body)
    {
        var request = CreateRequest(node, resource);

        request.Method = Method.Delete;

        if(body != null)
           request.AddParameter("text/plain", JsonConvert.SerializeObject(body), ParameterType.RequestBody);

        var response = await Client.ExecuteAsync(request);

        if (!response.IsSuccessful)
        {
            if (response.StatusCode != 0)
            {
                throw new WingsException(
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

    public async Task Put(Node node, string resource, object? body)
    {
        var request = CreateRequest(node, resource);

        request.Method = Method.Put;

        request.AddParameter("text/plain", JsonConvert.SerializeObject(body), ParameterType.RequestBody);

        var response = await Client.ExecuteAsync(request);

        if (!response.IsSuccessful)
        {
            if (response.StatusCode != 0)
            {
                throw new WingsException(
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

    private RestRequest CreateRequest(Node node, string resource)
    {
        var url = (node.Ssl ? "https" : "http") + $"://{node.Fqdn}:{node.HttpPort}/" + resource;

        var request = new RestRequest(url)
        {
            Timeout = 60 * 15
        };

        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Accept", "application/json");
        request.AddHeader("Authorization", "Bearer " + node.Token);

        return request;
    }
}