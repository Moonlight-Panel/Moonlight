namespace Moonlight.App.Models.Misc;

public class SubscriptionLimit
{
    public string Identifier { get; set; } = "";
    public int Amount { get; set; }
    public List<LimitOption> Options { get; set; } = new();

    public string? ReadValue(string key)
    {
        var d = Options.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.InvariantCultureIgnoreCase));
        return d?.Value;
    }
    
    public class LimitOption
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
    }
}