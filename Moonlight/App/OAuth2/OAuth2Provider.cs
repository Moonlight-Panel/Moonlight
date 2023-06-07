using Moonlight.App.Database.Entities;
using Moonlight.App.Models.Misc;

namespace Moonlight.App.OAuth2;

public abstract class OAuth2Provider
{
    public OAuth2ProviderConfig Config { get; set; }
    public string Url { get; set; }
    public IServiceScopeFactory ServiceScopeFactory { get; set; }
    public string DisplayName { get; set; }
    public bool CanBeLinked { get; set; } = false;
    
    public abstract Task<string> GetUrl();
    public abstract Task<User> HandleCode(string code);
    public abstract Task LinkToUser(User user, string code);
}