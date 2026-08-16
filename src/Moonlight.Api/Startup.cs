using Microsoft.Extensions.Logging.Console;
using Moonlight.Api.Infrastructure.Helpers;
using Moonlight.Shared.Shared;

namespace Moonlight.Api;

public static partial class Startup
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.TypeInfoResolver = DtoSerializer.Default);
        
        // Configure logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole(options => { options.FormatterName = nameof(AppConsoleFormatter); });
        builder.Logging.AddConsoleFormatter<AppConsoleFormatter, ConsoleFormatterOptions>();
        
        AddAuth(builder);
        AddDatabase(builder);
        AddCaching(builder);
        AddUsers(builder);
        AddRoles(builder);

        var app = builder.Build();

        UseAuth(app);

        app.MapControllers();

        await app.RunAsync();
    }
}