using MoonCore.Attributes;

namespace Moonlight.Features.StoreSystem.Services;

[Scoped]
public class StoreService
{
    private readonly IServiceProvider ServiceProvider;

    public StoreAdminService Admin => ServiceProvider.GetRequiredService<StoreAdminService>();
    public StoreOrderService Order => ServiceProvider.GetRequiredService<StoreOrderService>();
    public StorePaymentService Payment => ServiceProvider.GetRequiredService<StorePaymentService>();
    public StoreGiftService Gift => ServiceProvider.GetRequiredService<StoreGiftService>();

    public StoreService(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }
}