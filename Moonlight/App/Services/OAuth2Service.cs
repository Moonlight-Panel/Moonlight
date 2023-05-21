using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions;
using Moonlight.App.Helpers;
using Moonlight.App.Models.Misc;
using Moonlight.App.OAuth2;
using Moonlight.App.OAuth2.Providers;

namespace Moonlight.App.Services;

public class OAuth2Service
{
    public readonly Dictionary<string, OAuth2Provider> Providers = new();
    private readonly OAuth2ProviderConfig[] Configs;

    private readonly ConfigService ConfigService;
    private readonly IServiceScopeFactory ServiceScopeFactory;

    private readonly string OverrideUrl;
    private readonly bool EnableOverrideUrl;
    private readonly string AppUrl;

    public OAuth2Service(ConfigService configService, IServiceScopeFactory serviceScopeFactory)
    {
        ConfigService = configService;
        ServiceScopeFactory = serviceScopeFactory;

        var config = ConfigService.GetSection("Moonlight").GetSection("OAuth2");

        Configs = config.GetSection("Providers").Get<OAuth2ProviderConfig[]>();
        OverrideUrl = config.GetValue<string>("OverrideUrl");
        EnableOverrideUrl = config.GetValue<bool>("EnableOverrideUrl");
        AppUrl = configService.GetSection("Moonlight").GetValue<string>("AppUrl");
        
        // Register additional providers here
        RegisterOAuth2<DiscordOAuth2Provider>("discord");
        RegisterOAuth2<GoogleOAuth2Provider>("google");
    }

    private void RegisterOAuth2<T>(string id, string displayName = "")
    {
        var name = 
            string.IsNullOrEmpty(displayName) ?
                StringHelper.CapitalizeFirstCharacter(id) : displayName;
        
        if(Configs.All(x => x.Id != id))
            return;
        
        var provider = Activator.CreateInstance<T>()! as OAuth2Provider;

        if (provider == null)
            throw new Exception($"Unable to cast oauth2 provider '{typeof(T).Name}'");

        provider.Config = Configs.First(x => x.Id == id);
        provider.Url = GetAppUrl();
        provider.ServiceScopeFactory = ServiceScopeFactory;
        provider.DisplayName = name;

        Providers.Add(id, provider);
    }

    public async Task<string> GetUrl(string id)
    {
        if (Providers.All(x => x.Key != id))
            throw new DisplayException("Invalid oauth2 id");

        var provider = Providers[id];

        return await provider.GetUrl();
    }

    public async Task<User> HandleCode(string id, string code)
    {
        if (Providers.All(x => x.Key != id))
            throw new DisplayException("Invalid oauth2 id");

        var provider = Providers[id];

        return await provider.HandleCode(code);
    }

    private string GetAppUrl()
    {
        if (EnableOverrideUrl)
            return OverrideUrl;
        
        return AppUrl;
    }
}