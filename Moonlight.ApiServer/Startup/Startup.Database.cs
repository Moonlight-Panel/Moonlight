using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Extensions;

namespace Moonlight.ApiServer.Startup;

public static partial class Startup
{
    private static void AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbAutoMigrations();
        builder.Services.AddDatabaseMappings();

        builder.Services.AddScoped(typeof(DatabaseRepository<>));
    }
}