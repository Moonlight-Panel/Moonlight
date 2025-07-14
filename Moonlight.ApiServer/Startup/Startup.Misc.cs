using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Moonlight.ApiServer.Startup;

public partial class Startup
{
    private Task PrintVersion()
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

        return Task.CompletedTask;
    }

    private Task CreateStorage()
    {
        Directory.CreateDirectory("storage");
        Directory.CreateDirectory(Path.Combine("storage", "logs"));

        return Task.CompletedTask;
    }
    
    private Task RegisterCors()
    {
        var allowedOrigins = Configuration.Kestrel.AllowedOrigins;

        WebApplicationBuilder.Services.AddCors(options =>
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

        return Task.CompletedTask;
    }

    private Task UseCors()
    {
        WebApplication.UseCors();

        return Task.CompletedTask;
    }
}