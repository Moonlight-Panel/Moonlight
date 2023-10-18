namespace Moonlight.App.Services.Store;

public class StoreService
{
    private readonly IServiceProvider ServiceProvider;

    public StoreAdminService Admin => ServiceProvider.GetRequiredService<StoreAdminService>();

    public StoreService(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }
}