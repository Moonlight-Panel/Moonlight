using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions.Wings;
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

    private string GetApiUrl(Node node)
    {
        if(node.Ssl)
            return $"https://{node.Fqdn}:{node.HttpPort}/";
        else
            return $"http://{node.Fqdn}:{node.HttpPort}/";
        //return $"https://{node.Fqdn}:{node.HttpPort}/";
    }

    public async Task<T> Get<T>(Node node, string resource)
    {
        RestRequest request = new(GetApiUrl(node) + resource);

        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Accept", "application/json");
        request.AddHeader("Authorization", "Bearer " + node.Token);

        var response = await Client.GetAsync(request);

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
        RestRequest request = new(GetApiUrl(node) + resource);

        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Accept", "application/json");
        request.AddHeader("Authorization", "Bearer " + node.Token);

        var response = await Client.GetAsync(request);

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
        RestRequest request = new(GetApiUrl(node) + resource);

        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Accept", "application/json");
        request.AddHeader("Authorization", "Bearer " + node.Token);

        request.AddParameter("text/plain",
            JsonConvert.SerializeObject(body),
            ParameterType.RequestBody
        );

        var response = await Client.PostAsync(request);

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
        RestRequest request = new(GetApiUrl(node) + resource);

        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Accept", "application/json");
        request.AddHeader("Authorization", "Bearer " + node.Token);

       if(body != null)
           request.AddParameter("text/plain", JsonConvert.SerializeObject(body), ParameterType.RequestBody);

        var response = await Client.PostAsync(request);

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
        RestRequest request = new(GetApiUrl(node) + resource);

        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Accept", "application/json");
        request.AddHeader("Authorization", "Bearer " + node.Token);

        request.AddParameter("text/plain", body, ParameterType.RequestBody);

        var response = await Client.PostAsync(request);

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
        RestRequest request = new(GetApiUrl(node) + resource);

        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Accept", "application/json");
        request.AddHeader("Authorization", "Bearer " + node.Token);

       if(body != null)
           request.AddParameter("text/plain", JsonConvert.SerializeObject(body), ParameterType.RequestBody);

        var response = await Client.DeleteAsync(request);

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
        RestRequest request = new(GetApiUrl(node) + resource);

        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Accept", "application/json");
        request.AddHeader("Authorization", "Bearer " + node.Token);

        request.AddParameter("text/plain", JsonConvert.SerializeObject(body), ParameterType.RequestBody);

        var response = await Client.PutAsync(request);

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
}