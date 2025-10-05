using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database;
using Moonlight.ApiServer.Implementations.Diagnose;
using Moonlight.ApiServer.Implementations.Metrics;
using Moonlight.ApiServer.Interfaces;
using Moonlight.ApiServer.Models;
using Moonlight.ApiServer.Plugins;
using Moonlight.ApiServer.Services;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Moonlight.ApiServer.Implementations.Startup;

public class CoreStartup : IPluginStartup
{
    public void AddPlugin(WebApplicationBuilder builder)
    {
        var configuration = AppConfiguration.CreateEmpty();
        builder.Configuration.Bind(configuration);
        
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

        if (configuration.OpenTelemetry.Enable)
        {
            var openTel = builder.Services.AddOpenTelemetry();
            var openTelConfig = configuration.OpenTelemetry;

            var resourceBuilder = ResourceBuilder.CreateDefault();
            resourceBuilder.AddService(serviceName: "moonlight");

            openTel.ConfigureResource(x => x.AddService(serviceName: "moonlight"));

            if (openTelConfig.Metrics.Enable)
            {
                builder.Services.AddSingleton<MetricsBackgroundService>();
                builder.Services.AddHostedService(sp => sp.GetRequiredService<MetricsBackgroundService>());

                builder.Services.AddSingleton<IMetric, ApplicationMetric>();
                builder.Services.AddSingleton<IMetric, UsersMetric>();

                openTel.WithMetrics(providerBuilder =>
                {
                    providerBuilder.AddAspNetCoreInstrumentation();
                    providerBuilder.AddOtlpExporter();

                    if (openTelConfig.Metrics.EnablePrometheus)
                        providerBuilder.AddPrometheusExporter();

                    providerBuilder.AddMeter("moonlight");
                });
            }

            if (openTelConfig.Logs.Enable)
            {
                openTel.WithLogging();

                builder.Logging.AddOpenTelemetry(options =>
                {
                    options.SetResourceBuilder(resourceBuilder);
                    options.AddOtlpExporter();
                });
            }

            if (openTelConfig.Traces.Enable)
            {
                openTel.WithTracing(providerBuilder =>
                {
                    providerBuilder.AddAspNetCoreInstrumentation();

                    providerBuilder.AddOtlpExporter();
                });
            }
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
                    "/_content/MoonCore.Blazor.FlyonUi/ace/ace.js"
                ],
                Styles = ["/css/style.min.css"]
            });
        }

        #endregion
    }

    public void UsePlugin(WebApplication app)
    {
        var configuration = AppConfiguration.CreateEmpty();
        app.Configuration.Bind(configuration);
        
        #region Prometheus

        if (configuration.OpenTelemetry is { Enable: true, Metrics.EnablePrometheus: true })
            app.UseOpenTelemetryPrometheusScrapingEndpoint();

        #endregion
    }

    public void MapPlugin(WebApplication app)
    {
        var configuration = AppConfiguration.CreateEmpty();
        app.Configuration.Bind(configuration);
        
        if (configuration.Development.EnableApiDocs)
            app.MapSwagger("/api/swagger/{documentName}");
    }
}