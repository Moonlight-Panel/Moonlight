using Moonlight.App.Exceptions;
using Newtonsoft.Json;
using RestSharp;

namespace Moonlight.App.Helpers;

public class PaperApiHelper
{
    private string ApiUrl { get; set; }

    public PaperApiHelper()
    {
        ApiUrl = "https://api.papermc.io/v2/projects/";
    }
    
    public async Task<T> Get<T>(string url)
    {
        RestClient client = new();

        string requrl = "NONSET";

        if (ApiUrl.EndsWith("/"))
            requrl = ApiUrl + url;
        else
            requrl = ApiUrl + "/" + url;

        RestRequest request = new(requrl, Method.Get);

        request.AddHeader("Content-Type", "application/json");

        var response = await client.ExecuteAsync(request);

        if (!response.IsSuccessful)
        {
            if (response.StatusCode != 0)
            {
                throw new PaperException(
                    $"An error occured: ({response.StatusCode}) {response.Content}"
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