using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;

namespace Moonlight.Frontend.Features.Auth;

public static class TokenRefreshHandler
{
    public static async Task ValidatePrincipalAsync(CookieValidatePrincipalContext context)
    {
        var accessTokenExpiry = context.Properties.GetTokenValue("expires_at");

        if (!DateTimeOffset.TryParse(accessTokenExpiry, out var expiry))
            return;

        if (expiry > DateTimeOffset.UtcNow.AddMinutes(1))
            return;

        var refreshToken = context.Properties.GetTokenValue("refresh_token");
        
        if (string.IsNullOrEmpty(refreshToken))
        {
            context.RejectPrincipal();
            return;
        }

        var oidcOptions = context.HttpContext.RequestServices
            .GetRequiredService<IOptionsSnapshot<OpenIdConnectOptions>>()
            .Get(OpenIdConnectDefaults.AuthenticationScheme);

        if (oidcOptions.ConfigurationManager is null)
            throw new AggregateException("ConfigurationManager is not set for oidc provider");

        var discoveryDoc = await oidcOptions.ConfigurationManager
            .GetConfigurationAsync(context.HttpContext.RequestAborted);

        var httpClient = context.HttpContext.RequestServices
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient();

        var tokenResponse = await httpClient.PostAsync(
            discoveryDoc.TokenEndpoint,
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = oidcOptions.ClientId!,
                ["client_secret"] = oidcOptions.ClientSecret!,
                ["refresh_token"] = refreshToken,
            })
        );

        if (!tokenResponse.IsSuccessStatusCode)
        {
            context.RejectPrincipal();
            return;
        }

        var payload = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>();

        context.Properties.UpdateTokenValue(
            "access_token",
            payload.GetProperty("access_token").GetString()
        );

        context.Properties.UpdateTokenValue(
            "refresh_token",
            payload.GetProperty("refresh_token").GetString()
        );

        context.Properties.UpdateTokenValue(
            "expires_at",
            DateTimeOffset.UtcNow
                .AddSeconds(payload.GetProperty("expires_in").GetInt32())
                .ToString("o")
        );

        context.ShouldRenew = true;
    }
}