namespace Moonlight.App.Models.Misc;

public class SubscriptionLimit
{
    public string Identifier { get; set; } = "";
    public int Amount { get; set; }
    public List<LimitOption> Options { get; set; } = new();
    
    public class LimitOption
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
    }
}