using Microsoft.EntityFrameworkCore;

namespace Moonlight.Api.Infrastructure.Database;

public class MigrationService : IHostedLifecycleService
{
    private readonly ILogger<MigrationService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public MigrationService(ILogger<MigrationService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();

        var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();

        var pendingMigrationsEnum = await dataContext.Database.GetPendingMigrationsAsync(cancellationToken);
        var pendingMigrations = pendingMigrationsEnum.ToArray();

        if (pendingMigrations.Length > 0)
        {
            _logger.LogInformation("Pending migrations: {names}", string.Join(", ", pendingMigrations));

            await dataContext.Database.MigrateAsync(cancellationToken);
            
            _logger.LogInformation("Successfully applied all pending migrations");
        }
        else
            _logger.LogInformation("No pending migrations found");
    }

    #region Unused

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    #endregion
}