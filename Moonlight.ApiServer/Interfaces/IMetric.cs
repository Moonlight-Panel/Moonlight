using System.Diagnostics.Metrics;

namespace Moonlight.ApiServer.Interfaces;

public interface IMetric
{
    public Task InitializeAsync(Meter meter);
    public Task RunAsync(IServiceProvider provider, CancellationToken cancellationToken);
}