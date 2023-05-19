using System.ComponentModel;
using System.Text;
using Microsoft.JSInterop;
using RestSharp;

namespace Moonlight.App.Services.Interop;

public class ReCaptchaService
{
    private readonly IJSRuntime JsRuntime;
    private readonly ConfigService ConfigService;

    private readonly string SiteKey;
    private readonly string SecretKey;
    private readonly bool Enable = false;

    public Func<string, Task>? OnResponse { get; set; }
    public Func<Task>? OnValidResponse { get; set; }

    public ReCaptchaService(
        IJSRuntime jsRuntime,
        ConfigService configService)
    {
        JsRuntime = jsRuntime;
        ConfigService = configService;

        var recaptchaConfig = ConfigService
            .GetSection("Moonlight")
            .GetSection("Security")
            .GetSection("ReCaptcha");

        Enable = recaptchaConfig.GetValue<bool>("Enable");

        if (Enable)
        {
            SiteKey = recaptchaConfig.GetValue<string>("SiteKey");
            SecretKey = recaptchaConfig.GetValue<string>("SecretKey");
        }
    }

    public Task<bool> IsEnabled()
    {
        return Task.FromResult(Enable);
    }

    public async Task<string> Create(string elementId)
    {
        var page = DotNetObjectReference.Create(this);
        var res = await JsRuntime.InvokeAsync<object>("moonlight.recaptcha.render", elementId, SiteKey, page);
        
        return res.ToString() ?? "";
    }

    [JSInvokable, EditorBrowsable(EditorBrowsableState.Never)]
    public async void CallbackOnSuccess(string res)
    {
        if(OnResponse != null)
            await OnResponse.Invoke(res);

        var b = await Validate(res);

        if (b)
        {
            if(OnValidResponse != null)
                await OnValidResponse.Invoke();
        }
    }
    
    [JSInvokable, EditorBrowsable(EditorBrowsableState.Never)]
    public void CallbackOnExpired()
    {
        
    }

    private async Task<bool> Validate(string res)
    {
        var url = "https://www.google.com/recaptcha/api/siteverify";

        var client = new RestClient();
        var request = new RestRequest(url);
        request.AddParameter("secret", SecretKey);
        request.AddParameter("response", res);

        var resp = await client.PostAsync(request);
        
        var data = new ConfigurationBuilder().AddJsonStream(
            new MemoryStream(Encoding.ASCII.GetBytes(resp.Content))
        ).Build();
        return data.GetValue<bool>("success");
    }
}