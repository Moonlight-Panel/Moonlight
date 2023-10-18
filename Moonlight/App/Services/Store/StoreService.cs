namespace Moonlight.App.Services.Store;

public class StoreService
{
    private readonly IServiceProvider ServiceProvider;

    public StoreAdminService Admin => ServiceProvider.GetRequiredService<StoreAdminService>();
    public StoreOrderService Order => ServiceProvider.GetRequiredService<StoreOrderService>();

    public StoreService(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }
}