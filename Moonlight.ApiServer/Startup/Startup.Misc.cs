using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moonlight.ApiServer.Configuration;

namespace Moonlight.ApiServer.Startup;

public static partial class Startup
{
    private static void PrintVersionAsync()
    {
        // Fancy start console output... yes very fancy :>
        var rainbow = new Crayon.Rainbow(0.5);
        foreach (var c in "Moonlight")
        {
            Console.Write(
                rainbow
                    .Next()
                    .Bold()
                    .Text(c.ToString())
            );
        }

        Console.WriteLine();
    }

    private static void CreateStorageAsync()
    {
        Directory.CreateDirectory("storage");
        Directory.CreateDirectory(Path.Combine("storage", "logs"));
    }

    private static void AddMoonlightCors(this WebApplicationBuilder builder)
    {
        var configuration = AppConfiguration.CreateEmpty();
        builder.Configuration.Bind(configuration);
        
        var allowedOrigins = configuration.Kestrel.AllowedOrigins;

        builder.Services.AddCors(options =>
        {
            var cors = new CorsPolicyBuilder();

            if (allowedOrigins.Contains("*"))
            {
                cors.SetIsOriginAllowed(_ => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }
            else
            {
                cors.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            }

            options.AddDefaultPolicy(
                cors.Build()
            );
        });
    }

    private static void UseMoonlightCors(this WebApplication application)
    {
        application.UseCors();
    }
}