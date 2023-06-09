using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moonlight.App.Database;

namespace Moonlight.App.Diagnostics.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly DataContext DataContext;

    public DatabaseHealthCheck(DataContext dataContext)
    {
        DataContext = dataContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            await DataContext.Database.OpenConnectionAsync(cancellationToken);
            await DataContext.Database.CloseConnectionAsync();
            
            return HealthCheckResult.Healthy("Database is online");
        }
        catch (Exception e)
        {
            return HealthCheckResult.Unhealthy("Database is offline", e);
        }
    }
}