namespace Moonlight.Features.StoreSystem.Models.Abstractions;

public abstract class PaymentGateway
{
    public abstract string Name { get; }
    public abstract string Icon { get; }
    public abstract Task<string> Start(double price);
}