using Microsoft.OpenApi.Models;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database;
using Moonlight.ApiServer.Plugins;

namespace Moonlight.ApiServer.Implementations.Startup;

[PluginStartup]
public class CoreStartup : IPluginStartup
{
    public Task BuildApplication(IServiceProvider serviceProvider, IHostApplicationBuilder builder)
    {
        var configuration = serviceProvider.GetRequiredService<AppConfiguration>();
        
        #region Api Docs

        if (configuration.Development.EnableApiDocs)
        {
            builder.Services.AddEndpointsApiExplorer();
        
            // Configure swagger api specification generator and set the document title for the api docs to use
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("main", new OpenApiInfo()
                {
                    Title = "Moonlight API"
                });
                
                options.CustomSchemaIds(x => x.FullName);
                
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
            });
        }

        #endregion

        #region Database

        builder.Services.AddDbContext<CoreDataContext>();

        #endregion
        
        return Task.CompletedTask;
    }

    public Task ConfigureApplication(IServiceProvider serviceProvider, IApplicationBuilder app)
    {
        return Task.CompletedTask;
    }

    public Task ConfigureEndpoints(IServiceProvider serviceProvider, IEndpointRouteBuilder routeBuilder)
    {
        var configuration = serviceProvider.GetRequiredService<AppConfiguration>();
        
        if(configuration.Development.EnableApiDocs)
            routeBuilder.MapSwagger("/api/swagger/{documentName}");
        
        return Task.CompletedTask;
    }
}