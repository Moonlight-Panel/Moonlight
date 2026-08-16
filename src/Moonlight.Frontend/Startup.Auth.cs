using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Moonlight.Frontend.Features.Auth;

namespace Moonlight.Frontend;

public static partial class Startup
{
    private static void AddAuth(WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddOpenIdConnect(options =>
            {
                var authOptions = builder.Configuration.GetSection("Moonlight:Authentication").Get<AuthOptions>()!;

                options.Authority = authOptions.Authority;
                options.ClientId = authOptions.ClientId;
                options.ClientSecret = authOptions.ClientSecret;
                options.RequireHttpsMetadata = authOptions.RequireHttpsMetadata;
                options.ResponseType = OpenIdConnectResponseType.Code;

                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.SignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                options.SaveTokens = true;
            })
            .AddCookie(options =>
            {
                options.Events.OnValidatePrincipal += TokenRefreshHandler.ValidatePrincipalAsync;
            });

        builder.Services.AddAuthorization();

        builder.Services.AddTransient<IClaimsTransformation, NameClaimsTransformation>();

        builder.Services.Configure<HttpClientFactoryOptions>(
            "Api",
            options => options.HttpMessageHandlerBuilderActions.Add(messageHandlerBuilder =>
                messageHandlerBuilder.AdditionalHandlers.Add(
                    messageHandlerBuilder.Services.GetRequiredService<ApiTokenHandler>()
                )
            )
        );

        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<ApiTokenHandler>();

        // UI
        builder.Services.AddCascadingAuthenticationState();
    }

    private static void MapAuth(WebApplication app)
    {
        app.MapGet("/_auth/login", async (HttpContext context) =>
        {
            await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties()
            {
                RedirectUri = "/"
            });
        });
        
        app.MapGet("/_auth/logout", async (HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
            {
                RedirectUri = "/"
            });
        });
    }
}