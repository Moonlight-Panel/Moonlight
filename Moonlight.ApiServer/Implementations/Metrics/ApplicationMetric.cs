using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Moonlight.ApiServer.Interfaces;
using Moonlight.ApiServer.Services;

namespace Moonlight.ApiServer.Implementations.Metrics;

public class ApplicationMetric : IMetric
{
    private Gauge<long> MemoryUsage;
    private Gauge<int> CpuUsage;
    private Gauge<double> Uptime;
    
    public Task InitializeAsync(Meter meter)
    {
        MemoryUsage = meter.CreateGauge<long>("moonlight_memory_usage");
        CpuUsage = meter.CreateGauge<int>("moonlight_cpu_usage");
        Uptime = meter.CreateGauge<double>("moonlight_uptime");
        
        return Task.CompletedTask;
    }

    public async Task RunAsync(IServiceProvider provider, CancellationToken cancellationToken)
    {
        var applicationService = provider.GetRequiredService<ApplicationService>();

        var memory = await applicationService.GetMemoryUsageAsync();
        MemoryUsage.Record(memory);

        var uptime = await applicationService.GetUptimeAsync();
        Uptime.Record(uptime.TotalSeconds);

        var cpu = await applicationService.GetCpuUsageAsync();
        CpuUsage.Record(cpu);
    }
}