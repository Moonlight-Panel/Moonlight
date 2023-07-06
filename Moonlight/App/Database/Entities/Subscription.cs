using Moonlight.App.Models.Misc;

namespace Moonlight.App.Database.Entities;

public class Subscription
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public Currency Currency { get; set; } = Currency.USD;
    public double Price { get; set; }
    public string StripeProductId { get; set; } = "";
    public string StripePriceId { get; set; } = "";
    public string LimitsJson { get; set; } = "";
    public int Duration { get; set; } = 30;
}