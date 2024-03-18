using MoonCore.Attributes;
using Moonlight.Core.Models.Abstractions;
using Moonlight.Features.StoreSystem.Models.Abstractions;

namespace Moonlight.Features.StoreSystem.Services;

[Singleton]
public class StorePaymentService
{
    public readonly List<PaymentGateway> Gateways = new();
    
    public Task RegisterGateway(PaymentGateway gateway)
    {
        Gateways.Add(gateway);
        return Task.CompletedTask;
    }
}