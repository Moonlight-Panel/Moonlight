using System.Diagnostics.Metrics;

namespace Moonlight.ApiServer.Interfaces;

public interface IMetric
{
    public Task Initialize(Meter meter);
    public Task Run(IServiceProvider provider, CancellationToken cancellationToken);
}