using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace Moonlight.Core.Helpers;

public class HttpApiClient<TException> : IDisposable where TException : Exception
{
    private readonly HttpClient Client;
    private readonly string BaseUrl;

    public HttpApiClient(string baseUrl, string token)
    {
        Client = new();
        Client.DefaultRequestHeaders.Add("Authorization", token);

        BaseUrl = baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";
    }

    public async Task<string> Send(HttpMethod method, string path, string? body = null,
        string contentType = "text/plain")
    {
        var request = new HttpRequestMessage();

        request.RequestUri = new Uri(BaseUrl + path);
        request.Method = method;

        if (body != null)
            request.Content = new StringContent(body, Encoding.UTF8, new MediaTypeHeaderValue(contentType));

        var response = await Client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            await HandleRequestError(response, path);
            return "";
        }

        return await response.Content.ReadAsStringAsync();
    }

    private async Task HandleRequestError(HttpResponseMessage response, string path)
    {
        var content = await response.Content.ReadAsStringAsync();
        var message = $"[{path}] ({response.StatusCode}): {content}";
        var exception = Activator.CreateInstance(typeof(TException), message) as Exception;

        throw exception!;
    }

    #region GET

    public async Task<string> GetAsString(string path) =>
        await Send(HttpMethod.Get, path);

    public async Task<T> Get<T>(string path) =>
        JsonConvert.DeserializeObject<T>(await Send(HttpMethod.Get, path))!;

    #endregion

    #region POST

    public async Task<string> PostAsString(string path, string body, string contentType = "text/plain") =>
        await Send(HttpMethod.Post, path, body, contentType);

    public async Task<T> Post<T>(string path, object body) =>
        JsonConvert.DeserializeObject<T>(await Send(HttpMethod.Post, path, JsonConvert.SerializeObject(body),
            "application/json"))!;

    public async Task Post(string path, object? body = null) => await Send(HttpMethod.Post, path,
        body == null ? "" : JsonConvert.SerializeObject(body));

    #endregion

    #region PUT

    public async Task<string> PutAsString(string path, string body, string contentType = "text/plain") =>
        await Send(HttpMethod.Put, path, body, contentType);

    public async Task<T> Put<T>(string path, object body) =>
        JsonConvert.DeserializeObject<T>(await Send(HttpMethod.Put, path, JsonConvert.SerializeObject(body),
            "application/json"))!;

    public async Task Put(string path, object? body = null) => await Send(HttpMethod.Put, path,
        body == null ? "" : JsonConvert.SerializeObject(body));
    
    #endregion

    #region PATCH

    public async Task<string> PatchAsString(string path, string body, string contentType = "text/plain") =>
        await Send(HttpMethod.Patch, path, body, contentType);

    public async Task<T> Patch<T>(string path, object body) =>
        JsonConvert.DeserializeObject<T>(await Send(HttpMethod.Patch, path, JsonConvert.SerializeObject(body),
            "application/json"))!;

    public async Task Patch(string path, object? body = null) => await Send(HttpMethod.Patch, path,
        body == null ? "" : JsonConvert.SerializeObject(body));

    #endregion

    #region DELETE

    public async Task<string> DeleteAsString(string path) =>
        await Send(HttpMethod.Delete, path);

    public async Task<T> Delete<T>(string path) =>
        JsonConvert.DeserializeObject<T>(await Send(HttpMethod.Delete, path))!;

    #endregion

    public void Dispose()
    {
        Client.Dispose();
    }
}