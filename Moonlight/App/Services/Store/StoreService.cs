using Moonlight.App.Models.Abstractions;

namespace Moonlight.App.Services.Store;

public class StoreService
{
    private readonly IServiceProvider ServiceProvider;

    public StoreAdminService Admin => ServiceProvider.GetRequiredService<StoreAdminService>();
    public StoreOrderService Order => ServiceProvider.GetRequiredService<StoreOrderService>();
    public readonly List<PaymentGateway> Gateways = new();

    public StoreService(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public Task RegisterGateway(PaymentGateway gateway)
    {
        Gateways.Add(gateway);
        return Task.CompletedTask;
    }
}