using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MoonCore.Permissions;
using Moonlight.ApiServer.Implementations.LocalAuth;
using Moonlight.ApiServer.Services;

namespace Moonlight.ApiServer.Startup;

public partial class Startup
{
    private Task RegisterAuth()
    {
        WebApplicationBuilder.Services
            .AddAuthentication(options => { options.DefaultScheme = "MainScheme"; })
            .AddPolicyScheme("MainScheme", null, options =>
            {
                // If an api key is specified via the bearer auth header
                // we want to use the ApiKey scheme for authenticating the request
                options.ForwardDefaultSelector = context =>
                {
                    if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
                        return "Session";

                    var auth = authHeader.FirstOrDefault();

                    if (string.IsNullOrEmpty(auth) || !auth.StartsWith("Bearer "))
                        return "Session";

                    return "ApiKey";
                };
            })
            .AddJwtBearer("ApiKey", null, options =>
            {
                options.TokenValidationParameters = new()
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                        Configuration.Authentication.Secret
                    )),
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidateAudience = true,
                    ValidAudience = Configuration.PublicUrl,
                    ValidateIssuer = true,
                    ValidIssuer = Configuration.PublicUrl
                };

                options.Events = new JwtBearerEvents()
                {
                    OnTokenValidated = async context =>
                    {
                        var apiKeyAuthService = context
                            .HttpContext
                            .RequestServices
                            .GetRequiredService<ApiKeyAuthService>();

                        var result = await apiKeyAuthService.Validate(context.Principal);

                        if (!result)
                            context.Fail("API key has been deleted");
                    }
                };
            })
            .AddCookie("Session", null, options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromDays(Configuration.Authentication.Sessions.ExpiresIn);

                options.Cookie = new CookieBuilder()
                {
                    Name = Configuration.Authentication.Sessions.CookieName,
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

                    var result = await userSyncService.Sync(context.Principal);

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

                    var result = await userSyncService.Validate(context.Principal);

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

        WebApplicationBuilder.Services.AddAuthorization();
        
        WebApplicationBuilder.Services.AddAuthorizationPermissions(options =>
        {
            options.ClaimName = "Permissions";
            options.Prefix = "permissions:";
        });
        
        WebApplicationBuilder.Services.AddScoped<UserAuthService>();
        WebApplicationBuilder.Services.AddScoped<ApiKeyAuthService>();

        // Setup data protection storage within storage folder
        // so its persists in containers
        var dpKeyPath = Path.Combine("storage", "dataProtectionKeys");

        Directory.CreateDirectory(dpKeyPath);

        WebApplicationBuilder.Services
            .AddDataProtection()
            .PersistKeysToFileSystem(
                new DirectoryInfo(dpKeyPath)
            );

        WebApplicationBuilder.Services.AddScoped<UserDeletionService>();

        return Task.CompletedTask;
    }

    private Task UseAuth()
    {
        WebApplication.UseAuthentication();

        WebApplication.UseAuthorization();

        return Task.CompletedTask;
    }
}