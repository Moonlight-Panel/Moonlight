using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Interfaces;

namespace Moonlight.ApiServer.Services;

public class MetricsBackgroundService : BackgroundService
{
    private readonly ILogger<MetricsBackgroundService> Logger;
    private readonly IServiceProvider ServiceProvider;
    private readonly AppConfiguration Configuration;

    private readonly IMetric[] Metrics;
    private readonly Meter Meter;

    public MetricsBackgroundService(
        IServiceProvider serviceProvider,
        IMeterFactory meterFactory,
        IEnumerable<IMetric> metrics,
        ILogger<MetricsBackgroundService> logger,
        AppConfiguration configuration
    )
    {
        ServiceProvider = serviceProvider;
        Logger = logger;
        Configuration = configuration;

        Meter = meterFactory.Create("moonlight");

        Metrics = metrics.ToArray();
    }

    private async Task Initialize()
    {
        Logger.LogDebug(
            "Initializing metrics: {names}",
            string.Join(", ", Metrics.Select(x => x.GetType().FullName))
        );

        foreach (var metric in Metrics)
            await metric.Initialize(Meter);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Initialize();

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = ServiceProvider.CreateScope();

            foreach (var metric in Metrics)
            {
                try
                {
                    await metric.Run(scope.ServiceProvider, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Ignored
                }
                catch (Exception e)
                {
                    Logger.LogError(
                        "An unhandled error occured while collecting metric {name}: {e}",
                        metric.GetType().FullName,
                        e
                    );
                }
            }

            await Task.Delay(
                TimeSpan.FromSeconds(Configuration.OpenTelemetry.Metrics.Interval),
                stoppingToken
            );
        }
    }
}