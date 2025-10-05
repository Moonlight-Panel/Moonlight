using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MoonCore.Permissions;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Implementations.LocalAuth;
using Moonlight.ApiServer.Services;

namespace Moonlight.ApiServer.Startup;

public static partial class Startup
{
    private static void AddAuth(this WebApplicationBuilder builder)
    {
        var configuration = AppConfiguration.CreateEmpty();
        builder.Configuration.Bind(configuration);
        
        builder.Services
            .AddAuthentication(options => { options.DefaultScheme = "MainScheme"; })
            .AddPolicyScheme("MainScheme", null, options =>
            {
                // If an api key is specified via the bearer auth header
                // we want to use the ApiKey scheme for authenticating the request
                options.ForwardDefaultSelector = context =>
                {
                    var headers = context.Request.Headers;
       
                    // For regular api calls
                    if (headers.ContainsKey("Authorization"))
                        return "ApiKey";
 
                    // For websocket requests which cannot use the Authorization header
                    if (headers.Upgrade == "websocket" && headers.Connection == "Upgrade" && context.Request.Query.ContainsKey("access_token"))
                        return "ApiKey";

                    // Regular user traffic/auth
                    return "Session";
                };
            })
            .AddJwtBearer("ApiKey", null, options =>
            {
                options.TokenValidationParameters = new()
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                        configuration.Authentication.Secret
                    )),
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidateAudience = true,
                    ValidAudience = configuration.PublicUrl,
                    ValidateIssuer = true,
                    ValidIssuer = configuration.PublicUrl
                };

                options.Events = new JwtBearerEvents()
                {
                    OnTokenValidated = async context =>
                    {
                        var apiKeyAuthService = context
                            .HttpContext
                            .RequestServices
                            .GetRequiredService<ApiKeyAuthService>();

                        var result = await apiKeyAuthService.ValidateAsync(context.Principal);

                        if (!result)
                            context.Fail("API key has been deleted");
                    },
                    
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        if (!string.IsNullOrEmpty(accessToken))
                            context.Token = accessToken;
                        
                        return Task.CompletedTask;
                    }
                };
            })
            .AddCookie("Session", null, options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromDays(configuration.Authentication.Sessions.ExpiresIn);

                options.Cookie = new CookieBuilder()
                {
                    Name = configuration.Authentication.Sessions.CookieName,
                    Path = "/",
                    IsEssential = true,
                    SecurePolicy = CookieSecurePolicy.SameAsRequest
                };

                // As redirects won't work in our spa which uses API calls
                // we need to customize the responses when certain actions happen
                options.Events.OnRedirectToLogin = async context =>
                {
                    await Results.Problem(
                            title: "Unauthenticated",
                            detail: "You need to authenticate yourself to use this endpoint",
                            statusCode: 401
                        )
                        .ExecuteAsync(context.HttpContext);
                };

                options.Events.OnRedirectToAccessDenied = async context =>
                {
                    await Results.Problem(
                            title: "Permission denied",
                            detail: "You are missing the required permissions to access this endpoint",
                            statusCode: 403
                        )
                        .ExecuteAsync(context.HttpContext);
                };

                options.Events.OnSigningIn = async context =>
                {
                    var userSyncService = context
                        .HttpContext
                        .RequestServices
                        .GetRequiredService<UserAuthService>();

                    var result = await userSyncService.SyncAsync(context.Principal);

                    if (!result)
                        context.Principal = new();
                    else
                        context.Properties.IsPersistent = true;
                };

                options.Events.OnValidatePrincipal = async context =>
                {
                    var userSyncService = context
                        .HttpContext
                        .RequestServices
                        .GetRequiredService<UserAuthService>();

                    var result = await userSyncService.ValidateAsync(context.Principal);

                    if (!result)
                        context.RejectPrincipal();
                };
            })
            .AddScheme<LocalAuthOptions, LocalAuthHandler>(LocalAuthConstants.AuthenticationScheme, "Local Auth", options =>
            {
                options.ForwardAuthenticate = "Session";
                options.ForwardSignIn = "Session";
                options.ForwardSignOut = "Session";

                options.SignInScheme = "Session";
            });

        builder.Services.AddAuthorization();
        
        builder.Services.AddAuthorizationPermissions(options =>
        {
            options.ClaimName = "Permissions";
            options.Prefix = "permissions:";
        });
        
        builder.Services.AddScoped<UserAuthService>();
        builder.Services.AddScoped<ApiKeyAuthService>();

        // Setup data protection storage within storage folder
        // so its persists in containers
        var dpKeyPath = Path.Combine("storage", "dataProtectionKeys");

        Directory.CreateDirectory(dpKeyPath);

        builder.Services
            .AddDataProtection()
            .PersistKeysToFileSystem(
                new DirectoryInfo(dpKeyPath)
            );

        builder.Services.AddScoped<UserDeletionService>();
    }

    private static void UseAuth(this WebApplication application)
    {
        application.UseAuthentication();
        application.UseAuthorization();
    }
}