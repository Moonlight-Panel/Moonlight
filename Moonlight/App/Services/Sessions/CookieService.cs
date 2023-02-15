using Microsoft.JSInterop;

namespace Moonlight.App.Services.Sessions;

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
        var cValue = await GetCookie();
        if (string.IsNullOrEmpty(cValue)) return def;                

        var vals = cValue.Split(';');
        foreach (var val in vals)
            if(!string.IsNullOrEmpty(val) && val.IndexOf('=') > 0)
                if(val.Substring(1, val.IndexOf('=') - 1).Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
                    return val.Substring(val.IndexOf('=') + 1);
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