using Microsoft.Extensions.DependencyInjection;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Extensions;

namespace Moonlight.ApiServer.Startup;

public partial class Startup
{
    private Task RegisterDatabase()
    {
        WebApplicationBuilder.Services.AddDatabaseMappings();
        WebApplicationBuilder.Services.AddServiceCollectionAccessor();

        WebApplicationBuilder.Services.AddScoped(typeof(DatabaseRepository<>));

        return Task.CompletedTask;
    }

    private async Task PrepareDatabase()
    {
        await WebApplication.Services.EnsureDatabaseMigrated();

        WebApplication.Services.GenerateDatabaseMappings();
    }
}