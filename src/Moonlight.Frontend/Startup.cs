using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Moonlight.Frontend.Infrastructure.Api;
using Moonlight.Frontend.Infrastructure.Helpers;
using Moonlight.Frontend.Shared.Partials;
using ShadcnBlazor;
using ShadcnBlazor.Extras;

namespace Moonlight.Frontend;

public static partial class Startup
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddRazorComponents()
            .AddInteractiveServerComponents();
        
        // Configure logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole(options => { options.FormatterName = nameof(AppConsoleFormatter); });
        builder.Logging.AddConsoleFormatter<AppConsoleFormatter, ConsoleFormatterOptions>();
        
        builder.Services.AddShadcnBlazor();
        builder.Services.AddShadcnBlazorExtras();

        builder.Services.AddHttpContextAccessor();
        
        builder.Services.AddOptions<ApiOptions>().BindConfiguration("Moonlight:Api");
        builder.Services.AddHttpClient("Api", (provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<ApiOptions>>();
            client.BaseAddress = new Uri(options.Value.Url);
        });
        
        AddAuth(builder);
        
        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        MapAuth(app);

        await app.RunAsync();
    }
}