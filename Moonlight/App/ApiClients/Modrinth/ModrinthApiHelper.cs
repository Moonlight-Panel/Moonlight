using Newtonsoft.Json;
using RestSharp;

namespace Moonlight.App.ApiClients.Modrinth;

public class ModrinthApiHelper
{
    private readonly RestClient Client;

    public ModrinthApiHelper()
    {
        Client = new();
        Client.AddDefaultParameter(
            new HeaderParameter("User-Agent", "Moonlight-Panel/Moonlight (admin@endelon-hosting.de)")
        );
    }
    
    public async Task<T> Get<T>(string resource)
    {
        var request = CreateRequest(resource);

        request.Method = Method.Get;

        var response = await Client.ExecuteAsync(request);

        if (!response.IsSuccessful)
        {
            if (response.StatusCode != 0)
            {
                throw new ModrinthException(
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
    
    private RestRequest CreateRequest(string resource)
    {
        var url = "https://api.modrinth.com/v2/" + resource;

        var request = new RestRequest(url)
        {
            Timeout = 60 * 15
        };

        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Accept", "application/json");

        return request;
    }
}