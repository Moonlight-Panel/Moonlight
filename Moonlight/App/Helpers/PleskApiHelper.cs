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

        var response = await Client.GetAsync(request);

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
    
    public async Task<T> Post<T>(PleskServer server, string resource, object? body)
    {
        var request = CreateRequest(server, resource);

        request.AddParameter("text/plain",
            JsonConvert.SerializeObject(body),
            ParameterType.RequestBody
        );

        var response = await Client.PostAsync(request);

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
    
    public async Task Delete(PleskServer server, string resource, object? body)
    {
        var request = CreateRequest(server, resource);

        if(body != null)
            request.AddParameter("text/plain", JsonConvert.SerializeObject(body), ParameterType.RequestBody);

        var response = await Client.DeleteAsync(request);

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

    private RestRequest CreateRequest(PleskServer server, string resource)
    {
        RestRequest request = new(server.BaseUrl + "/" + resource);

        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Accept", "application/json");

        // Implementation of auth method using auth header and api key
        // https://docs.plesk.com/en-US/obsidian/api-rpc/about-rest-api.79359/#authentication-methods

        if (server.ApiKey.Contains(":"))
        {
            var base64 = Convert.ToBase64String(
                Encoding.ASCII.GetBytes(server.ApiKey)
            );

            request.AddHeader("Authorization", "Basic " + base64);
        }
        else
            request.AddHeader("X-API-Key", server.ApiKey);
        

        return request;
    }
}