using Moonlight.ApiServer.Interfaces.Startup;
using Moonlight.ApiServer.Services;

namespace Moonlight.ApiServer.Implementations.Startup;

public class CoreAssetStartup : IAppStartup
{
    private readonly BundleService BundleService;

    public CoreAssetStartup(BundleService bundleService)
    {
        BundleService = bundleService;
    }

    public Task BuildApp(IHostApplicationBuilder builder)
    {
        BundleService.BundleCss("css/core.min.css");
        
        return Task.CompletedTask;
    }

    public Task ConfigureApp(IApplicationBuilder app)
     => Task.CompletedTask;
}