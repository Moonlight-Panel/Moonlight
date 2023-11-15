using Microsoft.JSInterop;
using Moonlight.App.Helpers;

namespace Moonlight.App.Services.Interop;

public class CookieService
{
    private readonly IJSRuntime JsRuntime;
    private string Expires = "";

    public CookieService(IJSRuntime jsRuntime)
    {
        JsRuntime = jsRuntime;
        ExpireDays = 300;
    }

    public async Task SetValue(string key, string value, int? days = null)
    {
        var curExp = (days != null) ? (days > 0 ? DateToUTC(days.Value) : "") : Expires;
        await SetCookie($"{key}={value}; expires={curExp}; path=/");
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
        return await JsRuntime.InvokeAsync<string>("eval", $"document.cookie");
    }

    private int ExpireDays
    {
        set => Expires = DateToUTC(value);
    }

    private static string DateToUTC(int days) => DateTime.Now.AddDays(days).ToUniversalTime().ToString("R");
}