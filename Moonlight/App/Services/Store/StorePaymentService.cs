using Moonlight.App.Models.Abstractions;

namespace Moonlight.App.Services.Store;

public class StorePaymentService
{
    public readonly List<PaymentGateway> Gateways = new();
    
    public Task RegisterGateway(PaymentGateway gateway)
    {
        Gateways.Add(gateway);
        return Task.CompletedTask;
    }
}