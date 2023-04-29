using Moonlight.App.Database.Entities;
using Moonlight.App.Models.Plesk.Resources;
using Newtonsoft.Json;
using RestSharp;

namespace Moonlight.App.ApiClients.CloudPanel;

public class CloudPanelApiHelper
{
    private readonly RestClient Client;

    public CloudPanelApiHelper()
    {
        Client = new();
    }
    
    public async Task Post(Database.Entities.CloudPanel cloudPanel, string resource, object? body)
    {
        var request = CreateRequest(cloudPanel, resource);

        request.Method = Method.Post;

        if(body != null)
            request.AddParameter("text/plain", JsonConvert.SerializeObject(body), ParameterType.RequestBody);

        var response = await Client.ExecuteAsync(request);

        if (!response.IsSuccessful)
        {
            if (response.StatusCode != 0)
            {
                throw new CloudPanelException(
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
    
    public async Task Delete(Database.Entities.CloudPanel cloudPanel, string resource, object? body)
    {
        var request = CreateRequest(cloudPanel, resource);

        request.Method = Method.Delete;

        if(body != null)
            request.AddParameter("text/plain", JsonConvert.SerializeObject(body), ParameterType.RequestBody);

        var response = await Client.ExecuteAsync(request);

        if (!response.IsSuccessful)
        {
            if (response.StatusCode != 0)
            {
                throw new CloudPanelException(
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

    private RestRequest CreateRequest(Database.Entities.CloudPanel cloudPanel, string resource)
    {
        var url = $"{cloudPanel.ApiUrl}/" + resource;

        var request = new RestRequest(url);
        
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Accept", "application/json");
        request.AddHeader("Authorization", "Bearer " + cloudPanel.ApiKey);

        return request;
    }
}