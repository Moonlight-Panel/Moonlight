namespace Moonlight.Features.StoreSystem.Entities;

public class GiftCode
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public double Value { get; set; }
    public int Amount { get; set; }
}