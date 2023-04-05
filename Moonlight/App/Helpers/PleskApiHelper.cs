using System.Text;
using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions;
using Newtonsoft.Json;
using RestSharp;

namespace Moonlight.App.Helpers;

public class PleskApiHelper
{
    private readonly RestClient Client;

    public PleskApiHelper()
    {
        Client = new();
    }

    public async Task<T> Get<T>(PleskServer server, string resource)
    {
        var request = CreateRequest(server, resource);

        request.Method = Method.Get;

        var response = await Client.ExecuteAsync(request);

        if (!response.IsSuccessful)
        {
            if (response.StatusCode != 0)
            {
                throw new PleskException(
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

    public async Task<string> GetRaw(PleskServer server, string resource)
    {
        var request = CreateRequest(server, resource);

        request.Method = Method.Get;

        var response = await Client.ExecuteAsync(request);

        if (!response.IsSuccessful)
        {
            if (response.StatusCode != 0)
            {
                throw new PleskException(
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

    public async Task<T> Post<T>(PleskServer server, string resource, object? body)
    {
        var request = CreateRequest(server, resource);

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
                throw new PleskException(
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

    public async Task Post(PleskServer server, string resource, object? body)
    {
        var request = CreateRequest(server, resource);

        request.Method = Method.Post;

        if(body != null)
           request.AddParameter("text/plain", JsonConvert.SerializeObject(body), ParameterType.RequestBody);

        var response = await Client.ExecuteAsync(request);

        if (!response.IsSuccessful)
        {
            if (response.StatusCode != 0)
            {
                throw new PleskException(
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

    public async Task PostRaw(PleskServer server, string resource, object body)
    {
        var request = CreateRequest(server, resource);

        request.Method = Method.Post;

        request.AddParameter("text/plain", body, ParameterType.RequestBody);

        var response = await Client.ExecuteAsync(request);

        if (!response.IsSuccessful)
        {
            if (response.StatusCode != 0)
            {
                throw new PleskException(
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

    public async Task Delete(PleskServer server, string resource, object? body)
    {
        var request = CreateRequest(server, resource);

        request.Method = Method.Delete;

        if(body != null)
           request.AddParameter("text/plain", JsonConvert.SerializeObject(body), ParameterType.RequestBody);

        var response = await Client.ExecuteAsync(request);

        if (!response.IsSuccessful)
        {
            if (response.StatusCode != 0)
            {
                throw new PleskException(
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

    public async Task Put(PleskServer server, string resource, object? body)
    {
        var request = CreateRequest(server, resource);

        request.Method = Method.Put;

        request.AddParameter("text/plain", JsonConvert.SerializeObject(body), ParameterType.RequestBody);

        var response = await Client.ExecuteAsync(request);

        if (!response.IsSuccessful)
        {
            if (response.StatusCode != 0)
            {
                throw new PleskException(
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

    private RestRequest CreateRequest(PleskServer pleskServer, string resource)
    {
        var url = $"{pleskServer.ApiUrl}/" + resource;
        
        var request = new RestRequest(url);
        var ba = Convert.ToBase64String(Encoding.UTF8.GetBytes(pleskServer.ApiKey));
        
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Accept", "application/json");
        request.AddHeader("Authorization", "Basic " + ba);

        return request;
    }
}