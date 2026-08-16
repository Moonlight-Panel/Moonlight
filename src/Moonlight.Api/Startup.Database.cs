using Moonlight.Api.Infrastructure.Database;

namespace Moonlight.Api;

public static partial class Startup
{
    private static void AddDatabase(WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<DataContext>();
        builder.Services.AddOptions<DatabaseOptions>().BindConfiguration("Moonlight:Database");
        builder.Services.AddHostedService<MigrationService>();
    }
}