using Microsoft.OpenApi.Models;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database;
using Moonlight.ApiServer.Helpers;
using Moonlight.ApiServer.Interfaces.Startup;
using Moonlight.ApiServer.Services;

namespace Moonlight.ApiServer.Implementations.Startup;

public class CoreStartup : IPluginStartup
{
    private readonly AppConfiguration Configuration;
    private readonly BundleService BundleService;

    public CoreStartup(AppConfiguration configuration, BundleService bundleService)
    {
        Configuration = configuration;
        BundleService = bundleService;
    }

    public Task BuildApplication(IHostApplicationBuilder builder)
    {
        #region Api Docs

        if (Configuration.Development.EnableApiDocs)
        {
            builder.Services.AddEndpointsApiExplorer();
        
            // Configure swagger api specification generator and set the document title for the api docs to use
            builder.Services.AddSwaggerGen(options => options.SwaggerDoc("main", new OpenApiInfo()
            {
                Title = "Moonlight API"
            }));
        }

        #endregion

        #region Assets

        BundleService.BundleCss("css/core.min.css");

        #endregion
        
        return Task.CompletedTask;
    }

    public Task ConfigureApplication(IApplicationBuilder app)
    {
        return Task.CompletedTask;
    }

    public Task ConfigureDatabase(DatabaseContextCollection collection)
    {
        collection.Add<CoreDataContext>();
        
        return Task.CompletedTask;
    }

    public Task ConfigureEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        if(Configuration.Development.EnableApiDocs)
            routeBuilder.MapSwagger("/api/swagger/{documentName}");
        
        return Task.CompletedTask;
    }
}