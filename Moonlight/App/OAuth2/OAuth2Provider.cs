using Moonlight.App.Database.Entities;
using Moonlight.App.Models.Misc;

namespace Moonlight.App.OAuth2;

public abstract class OAuth2Provider
{
    public OAuth2ProviderConfig Config { get; set; }
    public string Url { get; set; }
    public IServiceScopeFactory ServiceScopeFactory { get; set; }
    public string DisplayName { get; set; }
    
    public abstract Task<string> GetUrl();
    public abstract Task<User> HandleCode(string code);
}