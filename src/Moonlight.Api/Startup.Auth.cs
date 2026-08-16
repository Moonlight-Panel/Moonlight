using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Moonlight.Api.Features.Auth;

namespace Moonlight.Api;

public static partial class Startup
{
    private static void AddAuth(WebApplicationBuilder builder)
    {
        var authOptions = builder.Configuration
            .GetSection("Moonlight:Authentication")
            .Get<AuthOptions>() ?? new AuthOptions();
        
        builder.Services.AddAuthentication("MainAuthentication")
            .AddPolicyScheme("MainAuthentication", null, options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    if (
                        !context.Request.Headers.TryGetValue("Authorization", out var headerValues) ||
                        headerValues.Count == 0
                    )
                    {
                        return JwtBearerDefaults.AuthenticationScheme;
                    }

                    var headerValue = headerValues[0];

                    if (string.IsNullOrWhiteSpace(headerValue) || !headerValue.StartsWith("moon_"))
                        return JwtBearerDefaults.AuthenticationScheme;

                    return "ApiKeyAuthentication";
                };
            })
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = true;
                options.RequireHttpsMetadata = authOptions.RequireHttpsMetadata;
                options.Audience = authOptions.Audience;
                options.Authority = authOptions.Authority;
            });

        builder.Services.AddAuthorization();

        builder.Services.AddScoped<UserSyncService>();
        
        builder.Services.AddTransient<IClaimsTransformation, NameClaimsTransformation>();
    }
    
    private static void UseAuth(WebApplication app)
    {
        app.UseAuthentication();
        app.UseMiddleware<SyncMiddleware>();
        app.UseAuthorization();
    }
}