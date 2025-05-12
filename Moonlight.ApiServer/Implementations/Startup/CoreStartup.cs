using Microsoft.OpenApi.Models;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database;
using Moonlight.ApiServer.Implementations.Diagnose;
using Moonlight.ApiServer.Interfaces;
using Moonlight.ApiServer.Interfaces.Startup;

namespace Moonlight.ApiServer.Implementations.Startup;

public class CoreStartup : IPluginStartup
{
    private readonly AppConfiguration Configuration;

    public CoreStartup(AppConfiguration configuration)
    {
        Configuration = configuration;
    }

    public Task BuildApplication(IHostApplicationBuilder builder)
    {
        #region Api Docs

        if (Configuration.Development.EnableApiDocs)
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

        #region Diagnose

        builder.Services.AddSingleton<IDiagnoseProvider, CoreDiagnoseProvider>();

        #endregion
        
        return Task.CompletedTask;
    }

    public Task ConfigureApplication(IApplicationBuilder app)
    {
        return Task.CompletedTask;
    }

    public Task ConfigureEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        if(Configuration.Development.EnableApiDocs)
            routeBuilder.MapSwagger("/api/swagger/{documentName}");
        
        return Task.CompletedTask;
    }
}