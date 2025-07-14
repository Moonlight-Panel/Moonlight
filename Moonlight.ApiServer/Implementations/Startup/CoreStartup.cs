using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database;
using Moonlight.ApiServer.Implementations.Diagnose;
using Moonlight.ApiServer.Implementations.Metrics;
using Moonlight.ApiServer.Interfaces;
using Moonlight.ApiServer.Models;
using Moonlight.ApiServer.Plugins;
using Moonlight.ApiServer.Services;
using OpenTelemetry.Metrics;

namespace Moonlight.ApiServer.Implementations.Startup;

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

        #region Diagnose

        builder.Services.AddSingleton<IDiagnoseProvider, CoreConfigDiagnoseProvider>();
        builder.Services.AddSingleton<IDiagnoseProvider, LogsDiagnoseProvider>();

        #endregion

        #region Prometheus

        if (configuration.Metrics.Enable)
        {
            builder.Services.AddSingleton<MetricsBackgroundService>();
            builder.Services.AddHostedService(sp => sp.GetRequiredService<MetricsBackgroundService>());

            builder.Services.AddSingleton<IMetric, ApplicationMetric>();
            builder.Services.AddSingleton<IMetric, UsersMetric>();

            builder.Services.AddOpenTelemetry()
                .WithMetrics(providerBuilder =>
                {
                    providerBuilder.AddPrometheusExporter();
                    providerBuilder.AddAspNetCoreInstrumentation();

                    providerBuilder.AddMeter("moonlight");
                });
        }

        #endregion

        #region Client / Frontend

        if (configuration.Frontend.EnableHosting)
        {
            builder.Services.AddSingleton(new FrontendConfigurationOption()
            {
                Scripts =
                [
                    "/_content/Moonlight.Client/js/moonlight.js", "/_content/MoonCore.Blazor.FlyonUi/moonCore.js",
                    "/_content/Moonlight.Client/ace/ace.js"
                ],
                Styles = ["/css/style.min.css"]
            });
        }

        #endregion

        return Task.CompletedTask;
    }

    public Task ConfigureApplication(IServiceProvider serviceProvider, IApplicationBuilder app)
    {
        var configuration = serviceProvider.GetRequiredService<AppConfiguration>();

        #region Prometheus

        if (configuration.Metrics.Enable)
            app.UseOpenTelemetryPrometheusScrapingEndpoint();

        #endregion

        return Task.CompletedTask;
    }

    public Task ConfigureEndpoints(IServiceProvider serviceProvider, IEndpointRouteBuilder routeBuilder)
    {
        var configuration = serviceProvider.GetRequiredService<AppConfiguration>();

        if (configuration.Development.EnableApiDocs)
            routeBuilder.MapSwagger("/api/swagger/{documentName}");

        return Task.CompletedTask;
    }
}