using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MoonCore.Extended.JwtInvalidation;
using MoonCore.Permissions;
using Moonlight.ApiServer.Implementations;
using Moonlight.ApiServer.Interfaces;
using Moonlight.ApiServer.Services;

namespace Moonlight.ApiServer.Startup;

public partial class Startup
{
    private Task RegisterAuth()
    {
        WebApplicationBuilder.Services
            .AddAuthentication("coreAuthentication")
            .AddJwtBearer("coreAuthentication", options =>
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
            });
        
        WebApplicationBuilder.Services.AddJwtBearerInvalidation("coreAuthentication");
        WebApplicationBuilder.Services.AddScoped<IJwtInvalidateHandler, UserAuthInvalidation>();

        WebApplicationBuilder.Services.AddAuthorization();
        
        WebApplicationBuilder.Services.AddAuthorizationPermissions(options =>
        {
            options.ClaimName = "permissions";
            options.Prefix = "permissions:";
        });

        // Add local oauth2 provider if enabled
        if (Configuration.Authentication.EnableLocalOAuth2)
            WebApplicationBuilder.Services.AddScoped<IOAuth2Provider, LocalOAuth2Provider>();

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