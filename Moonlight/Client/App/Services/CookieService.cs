using Microsoft.JSInterop;
using MoonCore.Attributes;

namespace Moonlight.Client.App.Services;

[Scoped]
public class CookieService
{
    private readonly IJSRuntime JsRuntime;

    public CookieService(IJSRuntime jsRuntime)
    {
        JsRuntime = jsRuntime;
    }

    public async Task SetValue(string key, string value, int days)
    {
        var utc = DateTime.UtcNow.AddDays(days).ToUniversalTime().ToString("R");
        await SetCookie($"{key}={value}; expires={utc}; path=/");
    }

    public async Task<string> GetValue(string key, string def = "")
    {
        var cookieString = await GetCookie();

        var cookieParts = cookieString.Split(";");

        foreach (var cookiePart in cookieParts)
        {
            if(string.IsNullOrEmpty(cookiePart))
                continue;
            
            var cookieKeyValue = cookiePart.Split("=")
                .Select(x => x.Trim()) // There may be spaces e.g. with the "AspNetCore.Culture" key
                .ToArray();

            if (cookieKeyValue.Length == 2)
            {
                if (cookieKeyValue[0] == key)
                    return cookieKeyValue[1];
            }
        }
        
        return def;
    }

    private async Task SetCookie(string value)
    {
        await JsRuntime.InvokeVoidAsync("eval", $"document.cookie = \"{value}\"");
    }

    private async Task<string> GetCookie()
    {
        return await JsRuntime.InvokeAsync<string>("eval", "document.cookie");
    }
}