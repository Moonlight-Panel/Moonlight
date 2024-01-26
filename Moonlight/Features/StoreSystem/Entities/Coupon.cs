namespace Moonlight.Features.StoreSystem.Entities;

public class Coupon
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public int Percent { get; set; }
    public int Amount { get; set; }
}