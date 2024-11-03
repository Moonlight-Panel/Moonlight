using Microsoft.OpenApi.Models;
using MoonCore.Services;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Interfaces.Startup;

namespace Moonlight.ApiServer.Implementations.Startup;

public class ApiDocsStartup : IAppStartup, IEndpointStartup
{
    private readonly ILogger<ApiDocsStartup> Logger;
    private readonly AppConfiguration AppConfiguration;

    public ApiDocsStartup(ILogger<ApiDocsStartup> logger, AppConfiguration appConfiguration)
    {
        Logger = logger;
        AppConfiguration = appConfiguration;
    }

    public Task BuildApp(IHostApplicationBuilder builder)
    {
        if(!AppConfiguration.Development.EnableApiDocs)
            return Task.CompletedTask;
        
        builder.Services.AddEndpointsApiExplorer();
        
        // Configure swagger api specification generator and set the document title for the api docs to use
        builder.Services.AddSwaggerGen(options => options.SwaggerDoc("main", new OpenApiInfo()
        {
            Title = "Moonlight API"
        }));
        
        return Task.CompletedTask;
    }

    public Task ConfigureApp(IApplicationBuilder app) => Task.CompletedTask;

    public Task ConfigureEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        if(!AppConfiguration.Development.EnableApiDocs)
            return Task.CompletedTask;
        
        routeBuilder.MapSwagger("/api/swagger/{documentName}");
        
        return Task.CompletedTask;
    }
}